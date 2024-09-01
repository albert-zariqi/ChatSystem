using Bogus;
using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using ChatSystem.Chat.Common.Response;
using ChatSystem.Utils.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Application.Sessions.Commands
{
    public class CreateSessionCommand : IRequest<ChatSessionResponse>
    {
        public CreateSessionCommand()
        {
        }

        public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, ChatSessionResponse>
        {
            private readonly IChatDbContext _chatDbContext;
            private readonly ICachingService _cachingService;

            public CreateSessionCommandHandler(
                IChatDbContext chatDbContext,
                ICachingService cachingService
                )
            {
                _chatDbContext = chatDbContext;
                _cachingService = cachingService;
            }

            public async Task<ChatSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
            {

                var shift = await GetShiftByCurrentTime();
                var shiftCapacity = await GetCurrentCapacity(shift.Id);


                if (!IsOverCapacity(shiftCapacity.CurrentCapacity, shiftCapacity.MaxCapacity))
                {
                    return new ChatSessionResponse
                    {
                        SessionId = Guid.NewGuid(),
                        Available = true
                    };

                    // Event: Place this shift in Queue
                }

                if (shift.IsDuringOfficeHours() && !shiftCapacity.OverflowAgentsRequested)
                {
                    var overflowAgents = await _chatDbContext.Agents.Include(x => x.Team).Where(x => x.Team.Name == "TEAM_O").CountAsync();
                    var juniorLevelFactor = await _chatDbContext.Seniorities.Where(x => x.Name == "JUNIOR").Select(x => x.Factor).FirstOrDefaultAsync();
                    var maximumConcurrencyNumber = 10;

                    var newCapacity = shiftCapacity.MaxCapacity + (overflowAgents * maximumConcurrencyNumber * juniorLevelFactor);
                    shiftCapacity.MaxCapacity = (int)newCapacity;

                    var shiftCapacityKey = ShiftCachingKeys.CapacityByShift(shift.Id);
                    await _cachingService.SetAsync(shiftCapacityKey, shiftCapacity, 43200);

                    // Event: Overflow agents requested!

                    if (IsOverCapacity(shiftCapacity.CurrentCapacity, shiftCapacity.MaxCapacity))
                    {
                        return new ChatSessionResponse
                        {
                            SessionId = null,
                            Available = false
                        };
                    }

                }

                return new ChatSessionResponse
                {
                    SessionId = null,
                    Available = false
                };
            }

            public bool IsOverCapacity(int currentCapacity, int maxCapacity)
            {
                return currentCapacity >= maxCapacity;
            }


            public async Task<ShiftCapacityCacheModel> GetCurrentCapacity(Guid shiftId)
            {
                var shiftCapacityKey = ShiftCachingKeys.CapacityByShift(shiftId);
                var shiftCapacity = await _cachingService.GetAsync<ShiftCapacityCacheModel?>(shiftCapacityKey);

                // Means no capacity has been calculated yet.
                // So we need to calculate the value. Then this value will be kept updated by the consumer
                if (shiftCapacity == null)
                {
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);
                    var shift = await GetShiftByCurrentTime();
                    var team = await _chatDbContext.Teams.Where(x => x.ShiftId == shift.Id && x.IsMainTeam).FirstAsync();

                    var agentsInShift = await _chatDbContext.Agents.Where(x => x.TeamId == team.Id).ToListAsync();
                    var seniorityLevels = await _chatDbContext.Seniorities.ToListAsync();

                    var maximumConcurrencyNumber = 10;
                    var teamLeadSeniority = seniorityLevels.Single(x => x.Name == "TEAM_LEAD");
                    var seniorSeniority = seniorityLevels.Single(x => x.Name == "SENIOR");
                    var midLevelSeniority = seniorityLevels.Single(x => x.Name == "MID_LEVEL");
                    var juniorSeniority = seniorityLevels.Single(x => x.Name == "JUNIOR");

                    var teamLeadsCount = agentsInShift.Count(x => x.SeniorityId == teamLeadSeniority.Id);
                    var seniorsCount = agentsInShift.Count(x => x.SeniorityId == seniorSeniority.Id);
                    var midLevelsCount = agentsInShift.Count(x => x.SeniorityId == midLevelSeniority.Id);
                    var juniorsCount = agentsInShift.Count(x => x.SeniorityId == juniorSeniority.Id);

                    var maxCapacity = (int)(
                        (teamLeadsCount * maximumConcurrencyNumber * teamLeadSeniority.Factor) +
                        (seniorsCount * maximumConcurrencyNumber * seniorSeniority.Factor) +
                        (midLevelsCount * maximumConcurrencyNumber * midLevelSeniority.Factor) +
                        (juniorsCount * maximumConcurrencyNumber * juniorSeniority.Factor));


                    ShiftCapacityCacheModel shiftCapacityCacheModel = new ShiftCapacityCacheModel();
                    shiftCapacityCacheModel.MaxCapacity = maxCapacity;
                    shiftCapacityCacheModel.CurrentCapacity = 0;

                    await _cachingService.SetAsync(shiftCapacityKey, shiftCapacityCacheModel, 43200);

                    return shiftCapacityCacheModel;
                }


                return shiftCapacity;
            }

            public async Task<Shift> GetShiftByCurrentTime()
            {
                TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);

                return await _chatDbContext.Shifts
                .Where(x => (x.StartHour < x.EndHour &&
                            (x.StartHour < currentTime.Hour ||
                            (x.StartHour == currentTime.Hour && x.StartMinute <= currentTime.Minute)) &&
                            (x.EndHour > currentTime.Hour ||
                            (x.EndHour == currentTime.Hour && x.EndMinute >= currentTime.Minute))) ||
                           (x.StartHour > x.EndHour &&
                            ((x.StartHour < currentTime.Hour ||
                            (x.StartHour == currentTime.Hour && x.StartMinute <= currentTime.Minute)) ||
                            (x.EndHour > currentTime.Hour ||
                            (x.EndHour == currentTime.Hour && x.EndMinute >= currentTime.Minute)))))
                .SingleOrDefaultAsync() ?? throw new AppException(new Utils.Errors.CustomError(System.Net.HttpStatusCode.NotFound, "shift_not_found", "Shift not found"));
            }

        }
    }
}

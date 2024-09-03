using Bogus;
using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using ChatSystem.Chat.Common.Response;
using ChatSystem.Messaging.Agents;
using ChatSystem.Messaging.Sessions;
using ChatSystem.Utils.Exceptions;
using MassTransit;
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
            private readonly IPublishEndpoint _publishEndpoint;

            public CreateSessionCommandHandler(
                IChatDbContext chatDbContext,
                ICachingService cachingService,
                IPublishEndpoint publishEndpoint
                )
            {
                _chatDbContext = chatDbContext;
                _cachingService = cachingService;
                _publishEndpoint = publishEndpoint;
            }

            public async Task<ChatSessionResponse> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
            {
                TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);

                var shift = await _chatDbContext.Shifts
                    .Where(x =>
                        (x.StartHour < x.EndHour && currentTime.Hour >= x.StartHour && currentTime.Hour <= x.EndHour) ||
                        (x.StartHour > x.EndHour &&
                            (currentTime.Hour >= x.StartHour || currentTime.Hour <= x.EndHour))
                    )
                    .Include(x => x.Teams)
                    .ThenInclude(x => x.Agents)
                    .ThenInclude(x => x.Seniority)
                    .SingleOrDefaultAsync() ?? throw new AppException(new Utils.Errors.CustomError(System.Net.HttpStatusCode.NotFound, "shift_not_found", "Shift not found"));

                //var shift = await GetShiftByCurrentTime();
                var shiftInformation = await GetShiftRealTimeInformation(shift.Id);

                if (!shift.IsOverNormalCapacity(shiftInformation.CurrentActiveSessions))
                {
                    ChatSession session = new ChatSession(shift.Id);
                    await _chatDbContext.ChatSessions.AddAsync(session);
                    await _chatDbContext.SaveChangesAsync();

                    await PublishNewSessionCreatedEvent(session);

                    return new ChatSessionResponse
                    {
                        SessionId = session.Id,
                        Available = true
                    };
                }

                if (shift.IsDuringOfficeHours() && !shiftInformation.OverflowAgentsRequested)
                {
                    if (shift.IsOverOverflowCapacity(shiftInformation.CurrentActiveSessions))
                    {
                        return new ChatSessionResponse
                        {
                            SessionId = null,
                            Available = false
                        };
                    }

                    ChatSession session = new ChatSession(shift.Id);
                    await _chatDbContext.ChatSessions.AddAsync(session);
                    await _chatDbContext.SaveChangesAsync();
                    await PublishNewSessionCreatedEvent(session);
                    await RequestOverflowAgents(shift.Id);

                    return new ChatSessionResponse
                    {
                        SessionId = Guid.NewGuid(),
                        Available = true
                    };
                }


                return new ChatSessionResponse
                {
                    SessionId = null,
                    Available = false
                };
            }

            private async Task PublishNewSessionCreatedEvent(ChatSession session)
            {
                await _publishEndpoint.Publish(new SessionCreatedMessage
                {
                    SessionId = session.Id,
                    ShiftId = session.ShiftId
                });
            }

            public bool IsOverCapacity(int currentCapacity, int maxCapacity)
            {
                return currentCapacity >= maxCapacity;
            }

            public async Task<ShiftCapacityCacheModel> GetShiftRealTimeInformation(Guid shiftId)
            {
                var shiftCapacityKey = ShiftCachingKeys.CapacityByShift(shiftId);
                var cacheModel = await _cachingService.GetAsync<ShiftCapacityCacheModel?>(shiftCapacityKey);

                // Means no capacity has been calculated yet.
                // So we need to calculate the value. Then this value will be kept updated by the consumer
                if (cacheModel == null)
                {
                    cacheModel = new ShiftCapacityCacheModel
                    {
                        CurrentActiveSessions = 0,
                        OverflowAgentsRequested = false
                    };
                    await _cachingService.SetAsync(shiftCapacityKey, cacheModel);
                    return cacheModel;
                }

                return cacheModel;
            }

            public async Task RequestOverflowAgents(Guid shiftId)
            {
                var shiftCapacityKey = ShiftCachingKeys.CapacityByShift(shiftId);
                var cacheModel = await _cachingService.GetAsync<ShiftCapacityCacheModel?>(shiftCapacityKey);
                cacheModel!.OverflowAgentsRequested = true;
                await _cachingService.SetAsync(shiftCapacityKey, cacheModel);

                await _publishEndpoint.Publish(new OverflowAgentsRequestMessage
                {
                    ShiftId = shiftId,
                    Message = "This is an automated request for overflow agents, due to a high-traffic situation in shift."
                });
            }

            public async Task<Shift> GetShiftByCurrentTime()
            {
                TimeOnly currentTime = TimeOnly.FromDateTime(DateTime.Now);

                return await _chatDbContext.Shifts
                    .Where(x => x.StartHour >= currentTime.Hour && x.StartMinute >= currentTime.Minute && x.EndHour <= currentTime.Hour && x.EndMinute < currentTime.Minute)
                    .Include(x => x.Teams)
                    .ThenInclude(x => x.Agents)
                    .ThenInclude(x => x.Seniority)
                    .SingleOrDefaultAsync() ?? throw new AppException(new Utils.Errors.CustomError(System.Net.HttpStatusCode.NotFound, "shift_not_found", "Shift not found"));
            }

        }
    }
}

using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.Common.Response;
using ChatSystem.Utils.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Application.Shifts.Queries
{
    public class GetShiftCapacityQuery : IRequest<ShiftCapacityResponse>
    {
        public Guid ShiftId { get; }

        public GetShiftCapacityQuery(Guid shiftId)
        {
            ShiftId = shiftId;
        }

        public class GetShiftCapacityQueryHandler : IRequestHandler<GetShiftCapacityQuery, ShiftCapacityResponse>
        {
            private readonly IChatDbContext _chatDbContext;

            public GetShiftCapacityQueryHandler(IChatDbContext chatDbContext)
            {
                _chatDbContext = chatDbContext;
            }

            public async Task<ShiftCapacityResponse> Handle(GetShiftCapacityQuery request, CancellationToken cancellationToken)
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

                return new ShiftCapacityResponse
                {
                    NormalQueueCapacity = shift.GetNormalQueueLimit(),
                    NormalConcurrentChatCapacity = shift.GetNormalConcurrentChatLimit(),
                    OverflowConcurrentChatCapacity = shift.GetOverflowConcurrentChatLimit(),
                    OverflowQueueCapacity = shift.GetOverflowQueueLimit(),
                    HasOverflowAgents = shift.Teams.Where(x => !x.IsMainTeam).Count() > 0
                };
            }
        }
    }
}

using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Utils.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Application.Agents.Commands
{
    public class AssignAgentCommand : IRequest
    {
        public AssignAgentCommand(Guid sessionId, string agentUsername)
        {
            ArgumentException.ThrowIfNullOrEmpty(agentUsername);
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException("Guid is not allowed to be default");
            }
            SessionId = sessionId;
            AgentUsername = agentUsername;
        }

        public Guid SessionId { get; }
        public string AgentUsername { get; }

        public class AssignAgentCommandHandler : IRequestHandler<AssignAgentCommand>
        {
            private readonly IChatDbContext _chatDbContext;
            private readonly ICachingService _cachingService;

            public AssignAgentCommandHandler(
                IChatDbContext chatDbContext,
                ICachingService cachingService
                )
            {
                _chatDbContext = chatDbContext;
                _cachingService = cachingService;
            }

            public async Task Handle(AssignAgentCommand request, CancellationToken cancellationToken)
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

                if (!(shift.Teams.SelectMany(x => x.Agents).Select(x => x.Username).Any(x => x == request.AgentUsername)))
                    throw new AppException(new Utils.Errors.CustomError("agent_not_in_shift", "agent_not_in_shift"));

                var chatSession = await _chatDbContext.ChatSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId) ?? throw new AppException(new Utils.Errors.CustomError("session_not_found", "session_not_found"));

                chatSession.AssignAgent(request.AgentUsername);
                chatSession.MarkActive();

                await _chatDbContext.SaveChangesAsync();
            }

        }
    }
}

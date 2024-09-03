using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using ChatSystem.Messaging.Sessions;
using ChatSystem.Utils.Exceptions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Application.Sessions.Commands
{
    public class EndSessionCommand : IRequest
    {
        public EndSessionCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; }

        public class CreateSessionCommandHandler : IRequestHandler<EndSessionCommand>
        {
            private readonly IChatDbContext _chatDbContext;
            private readonly ICachingService _cachingService;
            private readonly IPublishEndpoint _publishEndpoint;
            private readonly IRedisQueue _redisQueue;

            public CreateSessionCommandHandler(
                IChatDbContext chatDbContext,
                ICachingService cachingService,
                IPublishEndpoint publishEndpoint,
                IRedisQueue redisQueue
                )
            {
                _chatDbContext = chatDbContext;
                _cachingService = cachingService;
                _publishEndpoint = publishEndpoint;
                _redisQueue = redisQueue;
            }

            public async Task Handle(EndSessionCommand request, CancellationToken cancellationToken)
            {
                var chatSession = await _chatDbContext.ChatSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId) ?? throw new AppException(new Utils.Errors.CustomError("session_not_found", "session_not_found"));

                chatSession.MarkCompleted();

                await PublishSessionEndedEvent(chatSession);
            }

            private async Task PublishSessionEndedEvent(ChatSession session)
            {
                await _publishEndpoint.Publish(new SessionEndedMessage
                {
                    SessionId = session.ShiftId,
                    ShiftId = session.ShiftId,
                    Agent = session.AgentUsername!
                });
            }

        }
    }
}

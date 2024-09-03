using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.Common.Requests;
using MediatR;

namespace ChatSystem.Chat.API.Layers.Application.Sessions.Commands
{
    public class SendMessageCommand : IRequest
    {
        public SendMessageCommand(Guid sessionId, ChatMessageRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            SessionId = sessionId;
            Request = request;
        }

        public Guid SessionId { get; }
        public ChatMessageRequest Request { get; }

        public class SendMessageCommandHander : IRequestHandler<SendMessageCommand>
        {
            private readonly IChatDbContext _context;

            public SendMessageCommandHander(IChatDbContext context)
            {
                _context = context;
            }

            public async Task Handle(SendMessageCommand request, CancellationToken cancellationToken)
            {
                await _context.ChatMessages.AddAsync(new Domain.Entities.ChatMessage
                {
                    Id = Guid.NewGuid(),
                    Message = request.Request.Message,
                    Sender = request.Request.Sender,
                    Type = request.Request.FromAgent ? "Agent" : "User",
                    SessionId = request.SessionId,
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}

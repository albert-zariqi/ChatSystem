using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.Common.Response;
using MassTransit.Clients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Application.Sessions.Queries
{
    public class GetMessagesQuery : IRequest<List<MessageResponse>>
    {
        public Guid SessionId { get; }

        public GetMessagesQuery(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, List<MessageResponse>>
        {
            private readonly IChatDbContext _context;

            public GetMessagesQueryHandler(IChatDbContext context)
            {
                _context = context;
            }

            public async Task<List<MessageResponse>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
            {
                return await _context.ChatMessages.Where(x => x.SessionId == request.SessionId).OrderBy(x => x.CreatedOn).Select(x => new MessageResponse
                {
                    Sender = x.Sender,
                    FromAgent = x.Type == "Agent" ? true : false,
                    Message = x.Message,
                }).ToListAsync();
            }
        }
    }
}

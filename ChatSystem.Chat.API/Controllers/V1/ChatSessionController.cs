using ChatSystem.Chat.API.Layers.Application.Sessions.Commands;
using ChatSystem.Chat.API.Layers.Application.Sessions.Queries;
using ChatSystem.Chat.Common.Requests;
using ChatSystem.Chat.Common.Response;
using MassTransit.Clients;
using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Chat.API.Controllers.V1
{
    public class ChatSessionController : ApiBaseController
    {
        private readonly ISender _mediator;
        private readonly ILogger<ChatSessionController> _logger;

        public ChatSessionController(
            ISender mediator,
            ILogger<ChatSessionController> logger
            )
        {
            _mediator = mediator;
            _logger = logger;
        }


        [HttpPost("create")]
        [ProducesResponseType(201, Type = typeof(ChatSessionResponse))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> CreateSession()
        {
            _logger.LogInformation("Entering method {method}", nameof(CreateSession));

            var result = await _mediator.Send(new CreateSessionCommand());

            _logger.LogInformation("leaving method {method}", nameof(CreateSession));

            return Ok(result);
        }

        [HttpPost("end/{sessionId}")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            _logger.LogInformation("Entering method {method}", nameof(EndSession));

            await _mediator.Send(new EndSessionCommand(sessionId));

            _logger.LogInformation("leaving method {method}", nameof(EndSession));

            return Ok();
        }

        [HttpPost("{sessionId}/newmessage")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> SendMessage(Guid sessionId, ChatMessageRequest request)
        {
            _logger.LogInformation("Entering method {method}", nameof(SendMessage));

            await _mediator.Send(new SendMessageCommand(sessionId, request));

            _logger.LogInformation("leaving method {method}", nameof(SendMessage));

            return Ok();
        }

        [HttpGet("{sessionId}/messages")]
        [ProducesResponseType(201, Type = typeof(List<MessageResponse>))]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            _logger.LogInformation("Entering method {method}", nameof(GetMessages));

            var result = await _mediator.Send(new GetMessagesQuery(sessionId));

            _logger.LogInformation("leaving method {method}", nameof(GetMessages));

            return Ok(result);
        }
    }
}

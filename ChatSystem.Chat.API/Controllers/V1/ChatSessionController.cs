using ChatSystem.Chat.API.Layers.Application.Sessions.Commands;
using ChatSystem.Chat.Common.Response;
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


    }
}

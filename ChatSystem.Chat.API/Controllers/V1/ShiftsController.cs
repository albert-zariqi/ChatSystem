using ChatSystem.Chat.API.Layers.Application.Shifts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.Chat.API.Controllers.V1
{
    public class ShiftsController : ApiBaseController
    {
        private readonly ISender _mediator;
        private readonly ILogger<ChatSessionController> _logger;

        public ShiftsController(
            ISender mediator,
            ILogger<ChatSessionController> logger
            )
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("capacity/{shiftId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400, Type = typeof(ProblemDetails))]
        public async Task<IActionResult> GetShiftCapacity(Guid shiftId)
        {
            _logger.LogInformation("Entering method {method}", nameof(GetShiftCapacity));

            var result = await _mediator.Send(new GetShiftCapacityQuery(shiftId));

            _logger.LogInformation("leaving method {method}", nameof(GetShiftCapacity));

            return Ok(result);
        }
    }
}

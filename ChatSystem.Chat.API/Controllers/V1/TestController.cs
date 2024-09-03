using ChatSystem.Caching.Queue.SessionQueue;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Messaging.Agents;
using ChatSystem.Messaging.Sessions;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChatSystem.Chat.API.Controllers.V1
{
    public class TestController : ApiBaseController
    {
        private readonly ISender _mediator;
        private readonly ILogger<ChatSessionController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRedisQueue _redisQueue;

        public TestController(
            ISender mediator,
            ILogger<ChatSessionController> logger,
            IPublishEndpoint publishEndpoint,
            IRedisQueue redisQueue
            )
        {
            _mediator = mediator;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _redisQueue = redisQueue;
        }


        [HttpPost("push/{agent}/{sessionId}")]
        public async Task<IActionResult> CreateSession(string agent, Guid sessionId, string Message)
        {
            await _publishEndpoint.Publish(new ChatUpdateMessage
            {
                SessionId = sessionId,
                Agent = agent,
                Type = UpdateType.AgentInfo,
                Payload = Message
            });

            return Ok();
        }

        [HttpPost("push/{agent}/{sessionId}/{shiftId}")]
        public async Task<IActionResult> AgentEndSession(string agent, Guid sessionId, Guid shiftId)
        {
            await _publishEndpoint.Publish(new SessionEndedMessage
            {
                Agent = agent,
                SessionId = sessionId,
                ShiftId = shiftId,
            });

            await _redisQueue.Push(new StackExchange.Redis.RedisKey(sessionId.ToString()), new StackExchange.Redis.RedisValue(JsonConvert.SerializeObject(new SessionQueueModel
            {
                Agent = agent,
                Type = ChatSessionUpdateType.AgentDisconnected,
                Payload = "Agent has disconnected"
            })));

            return Ok();
        }
    }
}

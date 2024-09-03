using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
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
        private readonly ICachingService _cachingService;

        public TestController(
            ISender mediator,
            ILogger<ChatSessionController> logger,
            IPublishEndpoint publishEndpoint,
            IRedisQueue redisQueue,
            ICachingService cachingService
            )
        {
            _mediator = mediator;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _redisQueue = redisQueue;
            _cachingService = cachingService;
        }


        [HttpPost("create/{agent}/{sessionId}")]
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

        [HttpPost("end/{agent}/{sessionId}")]
        public async Task<IActionResult> AgentEndSession(string agent, Guid sessionId)
        {
            var shiftInfo = await _cachingService.GetAsync<SessionInShiftModel>(SessionCachingKeys.SessionInShift(sessionId));

            if (shiftInfo == null)
            {
                return BadRequest("No info found");
            }

            await _publishEndpoint.Publish(new SessionEndedMessage
            {
                Agent = agent,
                SessionId = sessionId,
                ShiftId = shiftInfo.ShiftId,
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

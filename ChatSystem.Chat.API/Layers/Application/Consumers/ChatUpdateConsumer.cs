using ChatSystem.Caching.Queue.SessionQueue;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Messaging.Agents;
using ChatSystem.Utils.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatSystem.Chat.API.Layers.Application.Consumers
{
    public class ChatUpdateConsumer : IConsumer<ChatUpdateMessage>
    {
        private readonly IChatDbContext _chatDbContext;
        private readonly ICachingService _cachingService;
        private readonly IRedisQueue _redisQueue;

        public ChatUpdateConsumer(
            IChatDbContext chatDbContext,
            ICachingService cachingService,
            IRedisQueue redisQueue
            )
        {
            _chatDbContext = chatDbContext;
            _cachingService = cachingService;
            _redisQueue = redisQueue;
        }

        public async Task Consume(ConsumeContext<ChatUpdateMessage> context)
        {
            var message = context.Message;
            var chatSession = await _chatDbContext.ChatSessions.FirstOrDefaultAsync(x => x.Id == message.SessionId) ?? throw new AppException(new Utils.Errors.CustomError("session_not_found", "session_not_found"));
            switch (message.Type)
            {
                case Messaging.Agents.UpdateType.AgentAssigned:
                    {
                        chatSession.AssignAgent(message.Agent);
                        chatSession.MarkActive();

                        break;
                    }
                case Messaging.Agents.UpdateType.AgentDisconnected:
                    {
                        chatSession.MarkInActive();

                        break;
                    }
                case Messaging.Agents.UpdateType.AgentInfo:
                    {

                        break;
                    }
                default:
                    throw new Exception("Unknown type of update");
            }

            var jsonPayload = JsonConvert.SerializeObject(new SessionQueueModel
            {
                Agent = message.Agent,
                Type = (ChatSessionUpdateType)message.Type,
                Payload = message.Payload
            });
            await _redisQueue.Push(new StackExchange.Redis.RedisKey(message.SessionId.ToString()), new StackExchange.Redis.RedisValue(jsonPayload));
        }
    }
}

using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Presentation.Hubs;
using ChatSystem.Presentation.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Quartz;
using ChatSystem.Caching.Queue.SessionQueue;

namespace ChatSystem.Presentation.BackgroundServices
{
    public class ChatPollingJob : IJob
    {
        private readonly IChatClient _chatPollingClient;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IRedisQueue _redisQueue;

        public ChatPollingJob(
            IChatClient chatPollingClient,
            IHubContext<ChatHub> hubContext,
            IRedisQueue redisQueue
            )
        {
            _chatPollingClient = chatPollingClient;
            _hubContext = hubContext;
            _redisQueue = redisQueue;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sessionId = context.MergedJobDataMap.GetString("SessionId");

            if (string.IsNullOrEmpty(sessionId))
                return;

            var result = await _redisQueue.Pop(new StackExchange.Redis.RedisKey(sessionId.ToString()));
            if (result.IsNullOrEmpty)
                return;

            var deserializedObject = JsonSerializer.Deserialize<SessionQueueModel>(result!)!;

            // Handle the result of polling
            await _hubContext.Clients.All.SendAsync("ReceiveChatResponse", new ChatResponse { AgentName = deserializedObject.Agent, Message = deserializedObject.Payload });
        }
    }
}

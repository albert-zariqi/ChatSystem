using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Presentation.Hubs;
using ChatSystem.Presentation.Services;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Quartz;
using ChatSystem.Caching.Queue.SessionQueue;
using ChatSystem.Caching.Models;
using ChatSystem.Caching.CachingKeys;

namespace ChatSystem.Presentation.BackgroundServices
{
    public class ChatPollingJob : IJob
    {
        private readonly IChatClient _chatPollingClient;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IRedisQueue _redisQueue;
        private readonly IChatClient _chatClient;
        private readonly ICachingService _cachingService;

        public ChatPollingJob(
            IChatClient chatPollingClient,
            IHubContext<ChatHub> hubContext,
            IRedisQueue redisQueue,
            IChatClient chatClient,
            ICachingService cachingService
            )
        {
            _chatPollingClient = chatPollingClient;
            _hubContext = hubContext;
            _redisQueue = redisQueue;
            _chatClient = chatClient;
            _cachingService = cachingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sessionId = context.MergedJobDataMap.GetString("SessionId");

            if (string.IsNullOrEmpty(sessionId))
                return;

            var shiftInfo = await _cachingService.GetAsync<SessionInShiftModel>(SessionCachingKeys.SessionInShift(Guid.Parse(sessionId)))!;

            var result = await _redisQueue.Pop(new StackExchange.Redis.RedisKey(sessionId.ToString()));
            if (result.IsNullOrEmpty)
            {

                if (shiftInfo == null)
                {
                    return;
                }

                if ((DateTimeOffset.UtcNow - shiftInfo.LastActivityTime).Minutes > 3)
                {
                    await _chatClient.ChatSession.EndSession(Guid.Parse(sessionId));
                    await _hubContext.Clients.All.SendAsync("ReceiveChatResponse",
                    new ChatResponse
                    {
                        AgentName = null,
                        Message = "Session closed due to inactivity",
                        Type = ChatSessionUpdateType.Inactivity.ToString(),
                    });
                }

                return;
            }

            var deserializedObject = JsonSerializer.Deserialize<SessionQueueModel>(result!)!;
            var persistMessageTask = _chatClient.ChatSession.SendChatMessage(Guid.Parse(sessionId), new ChatSystem.Chat.Common.Requests.ChatMessageRequest
            {
                FromAgent = true,
                Message = deserializedObject.Payload,
                Sender = deserializedObject.Agent
            });

            var updateSessionStateTask = _cachingService.SetAsync(SessionCachingKeys.SessionInShift(Guid.Parse(sessionId)), new SessionInShiftModel
            {
                LastActivityTime = DateTimeOffset.UtcNow,
                ShiftId = shiftInfo.ShiftId
            });

            // Handle the result of polling
            var sendToSignalRTask = _hubContext.Clients.Group(sessionId).SendAsync("ReceiveChatResponse",
            new ChatResponse
            {
                AgentName = deserializedObject.Agent,
                Message = deserializedObject.Payload,
                Type = deserializedObject.Type.ToString()
            });

            await Task.WhenAll(persistMessageTask, sendToSignalRTask);
        }
    }
}

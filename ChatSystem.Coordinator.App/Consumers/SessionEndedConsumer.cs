using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Coordinator.App.Services;
using ChatSystem.Messaging.Sessions;
using ChatSystem.Utils.Errors;
using ChatSystem.Utils.Exceptions;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Coordinator.App.Consumers
{
    public class SessionEndedConsumer : IConsumer<SessionEndedMessage>
    {
        private readonly IChatClient _chatClient;
        private readonly ICachingService _cachingService;
        private readonly IRedisQueue _redisQueue;

        public SessionEndedConsumer(
            IChatClient chatClient,
            ICachingService cachingService,
            IRedisQueue redisQueue
            )
        {
            _chatClient = chatClient;
            _cachingService = cachingService;
            _redisQueue = redisQueue;
        }

        public async Task Consume(ConsumeContext<SessionEndedMessage> context)
        {
            var message = context.Message;

            var activeAgentsInShift = await _cachingService.GetAsync<List<AgentsInShiftCacheModel>>(ShiftCachingKeys.AgentsInShift(message.ShiftId));

            if (activeAgentsInShift == null)
                throw new AppException(new CustomError("active_agents_in_shift_not_initialized", "Active agents in shift not initialized"));

            var agent = activeAgentsInShift.FirstOrDefault(x => x.Username == message.Agent);
            if (agent == null)
                throw new AppException(new CustomError("agent_not_found", "Agent not found"));

            agent.CurrentActiveSessions = agent.CurrentActiveSessions - 1;
            agent.CurrentActiveSessions = agent.CurrentActiveSessions < 0 ? 0 : agent.CurrentActiveSessions;

            // Overwrite the previous list
            await _cachingService.SetAsync(ShiftCachingKeys.AgentsInShift(message.ShiftId), activeAgentsInShift);

        }
    }
}

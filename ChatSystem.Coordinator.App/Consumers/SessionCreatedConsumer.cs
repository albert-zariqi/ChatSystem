using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
using ChatSystem.Caching.Queue.SessionQueue;
using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Coordinator.App.Services;
using ChatSystem.Messaging.Sessions;
using ChatSystem.Utils.Errors;
using ChatSystem.Utils.Exceptions;
using MassTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Coordinator.App.Consumers
{
    public class SessionCreatedConsumer : IConsumer<SessionCreatedMessage>
    {
        private readonly IChatClient _chatClient;
        private readonly ICachingService _cachingService;
        private readonly IRedisQueue _redisQueue;

        public SessionCreatedConsumer(
            IChatClient chatClient,
            ICachingService cachingService,
            IRedisQueue redisQueue
            )
        {
            _chatClient = chatClient;
            _cachingService = cachingService;
            _redisQueue = redisQueue;
        }

        public async Task Consume(ConsumeContext<SessionCreatedMessage> context)
        {
            var message = context.Message;

            var shiftRealTimeInfo = await _cachingService.GetAsync<ShiftCapacityCacheModel>(ShiftCachingKeys.CapacityByShift(message.ShiftId));

            if (shiftRealTimeInfo.OverConcurrencyLimit())
            {
                await _redisQueue.Push(new StackExchange.Redis.RedisKey("WaitingQueue"), JsonConvert.SerializeObject(new WaitingSessionModel
                {
                    SessionId = message.SessionId,
                    ShiftId = message.ShiftId,
                }));
                return;
            }

            var activeAgentsInShift = await _cachingService.GetAsync<List<AgentsInShiftCacheModel>>(ShiftCachingKeys.AgentsInShift(message.ShiftId));

            if (activeAgentsInShift == null)
                throw new AppException(new CustomError("active_agents_in_shift_not_initialized", "Active agents in shift not initialized"));

            var availableAgents = activeAgentsInShift.Where(x => x.CurrentActiveSessions < (10 * x.Factor)).ToList();

            ChatAssignment chatAssignment = new ChatAssignment(availableAgents);
            var agent = chatAssignment.RoundRobin();

            // Overwrite the previous list
            await _cachingService.SetAsync(ShiftCachingKeys.AgentsInShift(message.ShiftId), activeAgentsInShift);
            await _redisQueue.Push(new StackExchange.Redis.RedisKey(message.SessionId.ToString()), JsonConvert.SerializeObject(new SessionQueueModel
            {
                Agent = agent.Username,
                Payload = $"Agent {agent.Username} has connected",
                Type = ChatSessionUpdateType.AgentAssigned
            }));

        }
    }

    public class ChatAssignment
    {
        private Queue<AgentsInShiftCacheModel> _juniorAgents;
        private Queue<AgentsInShiftCacheModel> _midAgents;
        private Queue<AgentsInShiftCacheModel> _seniorAgents;
        private Queue<AgentsInShiftCacheModel> _teamLeads;

        public ChatAssignment(List<AgentsInShiftCacheModel> agents)
        {
            _juniorAgents = new Queue<AgentsInShiftCacheModel>(agents.Where(a => a.Seniority == "JUNIOR"));
            _midAgents = new Queue<AgentsInShiftCacheModel>(agents.Where(a => a.Seniority == "MID_LEVEL"));
            _seniorAgents = new Queue<AgentsInShiftCacheModel>(agents.Where(a => a.Seniority == "SENIOR"));
            _teamLeads = new Queue<AgentsInShiftCacheModel>(agents.Where(a => a.Seniority == "TEAM_LEAD"));
        }

        public AgentsInShiftCacheModel RoundRobin()
        {
            if (_juniorAgents.Any())
            {
                var agent = _juniorAgents.Dequeue();
                agent.CurrentActiveSessions = agent.CurrentActiveSessions + 1;
                _juniorAgents.Enqueue(agent); // Re-add to the end of the queue
                return agent;
            }
            else if (_midAgents.Any())
            {
                var agent = _midAgents.Dequeue();
                _midAgents.Enqueue(agent); // Re-add to the end of the queue
                agent.CurrentActiveSessions = agent.CurrentActiveSessions + 1;
                return agent;
            }
            else if (_seniorAgents.Any())
            {
                var agent = _seniorAgents.Dequeue();
                _seniorAgents.Enqueue(agent); // Re-add to the end of the queue
                agent.CurrentActiveSessions = agent.CurrentActiveSessions + 1;
                return agent;
            }
            else if (_teamLeads.Any())
            {
                var agent = _teamLeads.Dequeue();
                _seniorAgents.Enqueue(agent); // Re-add to the end of the queue
                agent.CurrentActiveSessions = agent.CurrentActiveSessions + 1;
                return agent;
            }

            throw new InvalidOperationException("No agents available to assign.");
        }
    }
}

using ChatSystem.Caching.CachingKeys;
using ChatSystem.Caching.Models;
using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Coordinator.App.Services;
using ChatSystem.Messaging.Agents;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Coordinator.App.Consumers
{
    public class OverflowAgentsConsumer : IConsumer<OverflowAgentsRequestMessage>
    {
        private readonly ICachingService _cachingService;
        private readonly IChatClient _chatClient;

        public OverflowAgentsConsumer(
            ICachingService cachingService,
            IChatClient chatClient
            )
        {
            _cachingService = cachingService;
            _chatClient = chatClient;
        }

        public async Task Consume(ConsumeContext<OverflowAgentsRequestMessage> context)
        {
            var message = context.Message;
            var cacheKey = ShiftCachingKeys.CapacityByShift(message.ShiftId);
            var shiftCapacityCacheModel = await _cachingService.GetAsync<ShiftCapacityCacheModel>(cacheKey);

            if (shiftCapacityCacheModel == null || shiftCapacityCacheModel.OverflowAgentsRequested)
                return;

            // Shift Capacity
            var shiftInfo = await _chatClient.Shift.GetShiftCapacity(message.ShiftId);
            await _cachingService.SetAsync(cacheKey, new ShiftCapacityCacheModel
            {
                CurrentActiveSessions = shiftCapacityCacheModel.CurrentActiveSessions,
                MaximumConcurrentSessions = shiftInfo.Data.OverflowConcurrentChatCapacity,
                MaximumQueueSize = shiftInfo.Data.OverflowQueueCapacity,
                OverflowAgentsRequested = true
            });
        }
    }
}

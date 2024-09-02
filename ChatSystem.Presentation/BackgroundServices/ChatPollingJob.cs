using ChatSystem.Coordinator.ApiClient.Abstractions;
using ChatSystem.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Quartz;

namespace ChatSystem.Presentation.BackgroundServices
{
    public class ChatPollingJob : IJob
    {
        private readonly IChatClient _chatPollingClient;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatPollingJob(
            IChatClient chatPollingClient,
            IHubContext<ChatHub> hubContext
            )
        {
            _chatPollingClient = chatPollingClient;
            _hubContext = hubContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var sessionId = context.MergedJobDataMap.GetString("SessionId");

            if (!string.IsNullOrEmpty(sessionId))
            {
                // Polling logic here
                //var result = await _chatPollingClient.PollSessionAsync(sessionId);

                // Handle the result of polling
                await _hubContext.Clients.All.SendAsync("ReceiveChatResponse", new ChatResponse { AgentName = "Shaban" });
            }
        }
    }
}

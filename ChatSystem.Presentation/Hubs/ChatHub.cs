using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.Presentation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatSessionManager _chatSessionManager;

        public ChatHub(ChatSessionManager chatSessionManager)
        {
            _chatSessionManager = chatSessionManager;
        }

        public async Task SendChatResponse(ChatResponse response)
        {
            await Clients.All.SendAsync("ReceiveChatResponse", response);
        }

        public async Task SubscribeToGroup(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        public async Task UnsubscribeFromGroup(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }

    }

    public class ChatResponse
    {
        public string AgentName { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
    }
}

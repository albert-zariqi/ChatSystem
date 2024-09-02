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

    }

    public class ChatResponse
    {
        public string AgentName { get; set; }
    }
}

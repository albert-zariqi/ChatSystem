using ChatSystem.Chat.Client.Abstractions.Requests;
using ChatSystem.Coordinator.ApiClient.Abstractions;

namespace ChatSystem.Coordinator.ApiClient.Clients
{
    public class ChatClient : IChatClient
    {
        public ChatClient(
            IChatSession chatSession
            )
        {
            ChatSession = chatSession;
        }

        public IChatSession ChatSession { get; }
    }
}

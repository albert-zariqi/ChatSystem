using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Chat.Client.Abstractions.Requests;

namespace ChatSystem.Chat.Client.Clients
{
    public class ChatClient : IChatClient
    {
        public IChatSession ChatSession { get; }
        public IShift Shift { get; }

        public ChatClient(
            IChatSession chatSession,
            IShift shift
            )
        {
            ChatSession = chatSession;
            Shift = shift;
        }
    }
}

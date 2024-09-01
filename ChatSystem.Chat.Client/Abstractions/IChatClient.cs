
using ChatSystem.Chat.Client.Abstractions.Requests;

namespace ChatSystem.Coordinator.ApiClient.Abstractions
{
    public interface IChatClient
    {
        IChatSession ChatSession { get; }

    }
}

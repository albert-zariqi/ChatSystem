
using ChatSystem.Chat.Client.Abstractions.Requests;

namespace ChatSystem.Chat.Client.Abstractions
{
    public interface IChatClient
    {
        IChatSession ChatSession { get; }
        IShift Shift { get; }

    }
}

using ChatSystem.Chat.Common.Response;

namespace ChatSystem.Presentation.ViewModels
{
    public class ChatViewModel
    {
        public List<MessageResponse> Messages { get; set; } = new List<MessageResponse>();
    }
}

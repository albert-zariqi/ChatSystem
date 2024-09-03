namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public string Message { get; set; }
        public string Sender { get; set; }
        public string Type { get; set; }
        public Guid SessionId { get; set; }
    }
}

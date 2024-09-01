using ChatSystem.Chat.API.Layers.Domain.Enums;

namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class ChatSession : BaseEntity
    {
        public Guid SessionId { get; set; }
        public ChatSessionStatus Status { get; set; }

        public Guid? AgentId { get; set; }

        #region Navigation

        public Agent Agent { get; set; }

        #endregion
    }
}

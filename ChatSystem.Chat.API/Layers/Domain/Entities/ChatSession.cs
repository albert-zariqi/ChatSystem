using ChatSystem.Chat.API.Layers.Domain.Enums;
using ChatSystem.Utils.Errors;
using ChatSystem.Utils.Exceptions;

namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class ChatSession : BaseEntity
    {
        public ChatSessionStatus Status { get; set; }
        public string? AgentUsername { get; set; }
        public Guid ShiftId { get; set; }

        public ChatSession(Guid shiftId)
        {
            Id = Guid.NewGuid();
            Status = ChatSessionStatus.WAITING;
            ShiftId = shiftId;
        }

        public void AssignAgent(string username)
        {
            AgentUsername = username;
        }

        public void MarkActive()
        {
            if (Status != ChatSessionStatus.WAITING)
                throw new AppException(new CustomError("invalid_status_move", "Invalid status move"));

            if (string.IsNullOrEmpty(AgentUsername))
                throw new AppException(new CustomError("invalid_agent", "Invalid agent"));

            Status = ChatSessionStatus.ACTIVE;
        }

        public void MarkInActive()
        {
            Status = ChatSessionStatus.INACTIVE;
        }

        public void MarkCompleted()
        {
            Status = ChatSessionStatus.COMPLETED;
        }
    }
}

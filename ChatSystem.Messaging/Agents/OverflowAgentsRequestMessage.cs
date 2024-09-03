using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Messaging.Agents
{
    public class OverflowAgentsRequestMessage : IMessage
    {
        public Guid ShiftId { get; set; }
        public string Message { get; set; }
    }
}

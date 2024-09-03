using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Messaging.Sessions
{
    public class SessionEndedMessage : IMessage
    {
        public Guid SessionId { get; set; }
        public Guid ShiftId { get; set; }
        public string Agent { get; set; }
    }
}

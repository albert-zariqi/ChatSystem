using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Messaging.Sessions
{
    public class SessionCreatedMessage : IMessage
    {
        public Guid SessionId { get; set; }
        public Guid ShiftId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Messaging.Agents
{
    public class ChatUpdateMessage
    {
        public Guid SessionId { get; set; }
        public string Agent { get; set; }
        public string Payload { get; set; }
        public UpdateType Type { get; set; }
    }

    public enum UpdateType
    {
        AgentAssigned = 0,
        AgentDisconnected = 1,
        AgentInfo = 2,
    }
}

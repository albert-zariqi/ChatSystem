using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Queue.SessionQueue
{
    public class SessionQueueModel
    {
        public string Agent { get; set; }
        public ChatSessionUpdateType Type { get; set; }
        public string Payload { get; set; }
    }

    public enum ChatSessionUpdateType
    {
        AgentAssigned = 0,
        AgentDisconnected = 1,
        AgentInfo = 2,
        Inactivity = 3,
    }

    public class WaitingSessionModel
    {
        public Guid SessionId { get; set; }
        public Guid ShiftId { get; set; }
    }
}

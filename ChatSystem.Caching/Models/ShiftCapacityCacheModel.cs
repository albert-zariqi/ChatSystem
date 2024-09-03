using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class ShiftCapacityCacheModel
    {
        public int CurrentActiveSessions { get; set; }
        public int MaximumConcurrentSessions { get; set; }
        public int MaximumQueueSize { get; set; }
        public bool OverflowAgentsRequested { get; set; }

        public bool OverConcurrencyLimit()
        {
            return CurrentActiveSessions > MaximumConcurrentSessions;
        }
    }
}

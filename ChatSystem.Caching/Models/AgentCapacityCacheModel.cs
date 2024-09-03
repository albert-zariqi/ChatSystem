using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class AgentCapacityCacheModel
    {
        public string Username { get; set; }
        public int CurrentActiveSessions { get; set; }
    }
}

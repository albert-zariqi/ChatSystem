using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class ShiftCapacityCacheModel
    {
        public int CurrentActiveSessions { get; set; }
        public bool OverflowAgentsRequested { get; set; }
    }
}

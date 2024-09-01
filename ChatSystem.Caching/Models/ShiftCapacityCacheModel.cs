using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class ShiftCapacityCacheModel
    {
        public int CurrentCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public bool OverflowAgentsRequested { get; set; }
    }
}

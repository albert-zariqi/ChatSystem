using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Response
{
    public class ShiftCapacityResponse
    {
        public int NormalCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public bool HasOverflowAgents { get; set; }
    }
}

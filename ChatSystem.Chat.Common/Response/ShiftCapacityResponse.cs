using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Response
{
    public class ShiftCapacityResponse
    {
        public int NormalQueueCapacity { get; set; }
        public int NormalConcurrentChatCapacity { get; set; }
        public int OverflowConcurrentChatCapacity { get; set; }
        public int OverflowQueueCapacity { get; set; }
        public bool HasOverflowAgents { get; set; }


    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Response
{
    public class ChatPollResponse
    {
        public bool AgentAvailable { get; set; }
        public string? AgentName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Requests
{
    public class ChatMessageRequest
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public bool FromAgent { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Response
{
    public class MessageResponse
    {
        public string Sender { get; set; }
        public bool FromAgent { get; set; }
        public string Message { get; set; }
    }
}

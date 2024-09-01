using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Chat.Common.Response
{
    public class ChatSessionResponse
    {
        public bool Available { get; set; }
        public Guid? SessionId { get; set; }
    }
}

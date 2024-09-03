using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class SessionInShiftModel
    {
        public Guid ShiftId { get; set; }
        public DateTimeOffset LastActivityTime { get; set; }
    }
}

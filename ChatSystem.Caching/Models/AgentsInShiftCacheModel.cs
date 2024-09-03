using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.Models
{
    public class AgentsInShiftCacheModel
    {
        public string Username { get; set; }
        public string Seniority { get; set; }
        public decimal Factor { get; set; }
        public int CurrentActiveSessions { get; set; }
    }
}

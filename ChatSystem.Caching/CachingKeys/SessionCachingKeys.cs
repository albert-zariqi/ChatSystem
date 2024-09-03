using System;
using System.Collections.Generic;
using System.Text;

namespace ChatSystem.Caching.CachingKeys
{
    public static class SessionCachingKeys
    {
        public static string SessionInShift(Guid sessionId) => $"session-in-shift:{sessionId}";

    }
}

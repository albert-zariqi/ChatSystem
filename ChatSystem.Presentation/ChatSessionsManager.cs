using System.Collections.Concurrent;

namespace ChatSystem.Presentation
{
    public class ChatSessionManager
    {
        private readonly ConcurrentDictionary<Guid, (bool IsActive, DateTime ExpiryDate)> _sessions = new();

        public void AddSession(Guid sessionId, TimeSpan duration)
        {
            var expiryDate = DateTime.UtcNow.Add(duration);
            _sessions.TryAdd(sessionId, (true, expiryDate));
        }

        public bool SessionExists(Guid sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var sessionInfo))
            {
                // Check if the session has expired
                if (sessionInfo.ExpiryDate < DateTime.UtcNow)
                {
                    // If expired, remove the session and return false
                    _sessions.TryRemove(sessionId, out _);
                    return false;
                }
                return true;
            }
            return false;
        }

        public void RemoveSession(Guid sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public IEnumerable<Guid> GetAllSessions()
        {
            // Clean up expired sessions
            foreach (var sessionId in _sessions.Keys.ToList())
            {
                if (SessionExists(sessionId))
                {
                    yield return sessionId;
                }
            }
        }

        public bool TryExtendSession(Guid sessionId, TimeSpan additionalTime)
        {
            if (_sessions.TryGetValue(sessionId, out var sessionInfo))
            {
                var newExpiryDate = sessionInfo.ExpiryDate.Add(additionalTime);
                _sessions[sessionId] = (sessionInfo.IsActive, newExpiryDate);
                return true;
            }
            return false;
        }
    }
}

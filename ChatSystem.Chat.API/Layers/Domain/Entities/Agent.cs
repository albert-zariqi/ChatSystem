namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class Agent : BaseEntity
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public Guid SeniorityId { get; set; }
        public Guid TeamId { get; set; }

        #region Navigations

        public Seniority Seniority { get; set; }
        public Team Team { get; set; }
        public List<ChatSession> ChatSessions { get; set; }
        #endregion
    }
}

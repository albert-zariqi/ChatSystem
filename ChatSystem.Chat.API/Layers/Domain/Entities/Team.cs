namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class Team : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ShiftId { get; set; }
        public bool IsMainTeam { get; set; }

        #region Navigations

        public Shift Shift { get; set; }
        public List<Agent> Agents { get; set; }

        #endregion
    }
}

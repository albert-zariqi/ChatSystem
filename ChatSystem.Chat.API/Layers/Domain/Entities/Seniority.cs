namespace ChatSystem.Chat.API.Layers.Domain.Entities
{
    public class Seniority : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Factor { get; set; }
    }
}

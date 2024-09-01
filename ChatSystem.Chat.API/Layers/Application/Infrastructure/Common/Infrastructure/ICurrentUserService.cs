namespace ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure
{
    public interface ICurrentUserService
    {
        public string UserId { get; }
        public Guid UserGuid { get; }
        public string Username { get; }
    }
}

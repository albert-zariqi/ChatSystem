namespace ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services
{
    public interface ICachingService
    {
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task SetAsync<T>(string key, T entity, int expiryTimeInSeconds = 86400);
    }
}

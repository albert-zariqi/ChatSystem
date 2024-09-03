using StackExchange.Redis;

namespace ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services
{
    public interface ICachingService
    {
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task SetAsync<T>(string key, T entity, int expiryTimeInSeconds = 86400);
    }

    public interface IRedisQueue
    {
        Task<RedisValue> Pop(RedisKey queueName);
        Task Push(RedisKey queueName, RedisValue value);
        Task RemoveAsync(RedisKey queueName);
    }
}

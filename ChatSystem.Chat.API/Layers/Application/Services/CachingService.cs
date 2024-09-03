using ChatSystem.Chat.API.Layers.Application.Configurations;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ChatSystem.Chat.API.Layers.Application.Services
{
    public class RedisQueue : IRedisQueue
    {
        private readonly IDatabase _db;
        public RedisQueue(
            IOptions<ConnectionStrings> connectionStrings
            )
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionStrings.Value.RedisConnection);
            _db = redis.GetDatabase();
        }

        public async Task<RedisValue> Pop(RedisKey queueName)
        {
            return await _db.ListRightPopAsync(queueName);
        }

        public async Task Push(RedisKey queueName, RedisValue value)
        {
            await _db.ListRightPushAsync(queueName, value);
        }
        public async Task RemoveAsync(RedisKey queueName)
        {
            await _db.KeyDeleteAsync(queueName);
        }
    }
    public class CachingService : ICachingService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CachingService> _logger;

        public CachingService(
            IDistributedCache distributedCache,
            ILogger<CachingService> logger
            )
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var entityString = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(entityString) ? JsonConvert.DeserializeObject<T>(entityString, new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            }) : default;
        }

        public async Task SetAsync<T>(string key, T entity, int expiryTimeInSeconds = 86400)
        {

            var serializedObject = JsonConvert.SerializeObject(entity);
            await _distributedCache.SetStringAsync(key, serializedObject, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiryTimeInSeconds)
            });
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }
    }
}

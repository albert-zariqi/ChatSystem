using ChatSystem.Coordinator.App.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Coordinator.App.Services
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

        public async Task RemoveQueue(RedisKey queueName)
        {
            await _db.KeyDeleteAsync(queueName);
        }
    }
    public class CachingService : ICachingService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CachingService> _logger;
        private readonly IOptions<ConnectionStrings> _connectionStrings;

        public CachingService(
            IDistributedCache distributedCache,
            ILogger<CachingService> logger,
            IOptions<ConnectionStrings> connectionStrings
            )
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _connectionStrings = connectionStrings;
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
        Task RemoveQueue(RedisKey queueName);
    }
}

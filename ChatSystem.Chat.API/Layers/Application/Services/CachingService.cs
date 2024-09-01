using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace ChatSystem.Chat.API.Layers.Application.Services
{
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

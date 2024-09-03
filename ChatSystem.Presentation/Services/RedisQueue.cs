using ChatSystem.Presentation.Configurations;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ChatSystem.Presentation.Services
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
    }
}

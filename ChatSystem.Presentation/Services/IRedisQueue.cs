using StackExchange.Redis;

namespace ChatSystem.Presentation.Services
{
    public interface IRedisQueue
    {
        Task<RedisValue> Pop(RedisKey queueName);
        Task Push(RedisKey queueName, RedisValue value);
    }
}

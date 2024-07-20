namespace BookWorm.Ordering.Infrastructure.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);

    Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory);

    Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan expiration);
}

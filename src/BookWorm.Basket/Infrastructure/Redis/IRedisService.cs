namespace BookWorm.Basket.Infrastructure.Redis;

public interface IRedisService
{
    Task<T?> HashGetAsync<T>(string key, string hashKey);

    Task<T> HashSetAsync<T>(string key, string hashKey, T value);

    Task<IEnumerable<T>> HashGetAllAsync<T>(string key);

    Task HashRemoveAsync(string key, string hashKey);
}

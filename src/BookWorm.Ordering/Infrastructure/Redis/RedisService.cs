using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace BookWorm.Ordering.Infrastructure.Redis;

public sealed class RedisService(IConfiguration configuration) : IRedisService
{
    private const int DefaultExpirationTime = 3600;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer = new(
        () =>
            ConnectionMultiplexer.Connect(
                configuration.GetConnectionString(ServiceName.Redis)
                    ?? throw new InvalidOperationException()
            )
    );

    private ConnectionMultiplexer ConnectionMultiplexer => _connectionMultiplexer.Value;

    private IDatabase Database
    {
        get
        {
            _connectionLock.Wait();

            try
            {
                return ConnectionMultiplexer.GetDatabase();
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory)
    {
        return await GetOrSetAsync(key, valueFactory, TimeSpan.FromSeconds(DefaultExpirationTime));
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan expiration)
    {
        Guard.Against.NullOrEmpty(key);

        var cachedValue = await Database.StringGetAsync(key);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            return GetByteToObject<T>(cachedValue);
        }

        var newValue = valueFactory();
        if (newValue is not null)
        {
            await Database.StringSetAsync(key, JsonSerializer.Serialize(newValue), expiration);
        }

        return newValue;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await Database.StringGetAsync(key);

        return !string.IsNullOrEmpty(cachedValue) ? GetByteToObject<T>(cachedValue) : default;
    }

    private static T GetByteToObject<T>(RedisValue value)
    {
        var result = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value!));
        return result is null
            ? throw new InvalidOperationException("Deserialization failed.")
            : result;
    }
}

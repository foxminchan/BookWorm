using Ardalis.GuardClauses;
using System.Text.Json;
using StackExchange.Redis;
using System.Text;

namespace BookWorm.Basket.Infrastructure.Redis;

public sealed class RedisService(IConfiguration configuration) : IRedisService
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private readonly Lazy<ConnectionMultiplexer> _connectionMultiplexer = new(() =>
        ConnectionMultiplexer.Connect(
            configuration.GetConnectionString("redis") ?? throw new InvalidOperationException()));

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

    public async Task<T?> HashGetAsync<T>(string key, string hashKey)
    {
        Guard.Against.NullOrEmpty(key);

        Guard.Against.NullOrEmpty(hashKey);

        var value = await Database.HashGetAsync(key, hashKey.ToLower());

        return !value.IsNull
            ? GetByteToObject<T>(value)
            : default;
    }

    public async Task<T> HashSetAsync<T>(string key, string hashKey, T value)
    {
        Guard.Against.NullOrEmpty(key);

        Guard.Against.NullOrEmpty(hashKey);

        await Database.HashSetAsync(key, hashKey.ToLower(), JsonSerializer.Serialize(value));

        return value;
    }

    public async Task<IEnumerable<T>> HashGetAllAsync<T>(string key)
    {
        var values = await Database.HashGetAllAsync(key);

        return values.Length != 0
            ? values.Select(x => GetByteToObject<T>(x.Value)).ToArray()
            : [];
    }

    public async Task HashRemoveAsync(string key, string hashKey)
        => await Database.HashDeleteAsync(key, hashKey.ToLower());

    private static T GetByteToObject<T>(RedisValue value)
    {
        var result = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value!));
        return result is null ? throw new InvalidOperationException("Deserialization failed.") : result;
    }
}

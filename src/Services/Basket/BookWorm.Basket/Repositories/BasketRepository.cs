using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace BookWorm.Basket.Repositories;

public sealed class BasketRepository(ILogger<BasketRepository> logger, IConnectionMultiplexer redis)
    : IBasketRepository
{
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private static RedisKey BasketKey => "/basket"u8.ToArray();

    public async Task<CustomerBasket?> GetBasketAsync(string id)
    {
        var database = await GetDatabaseAsync();
        var data = await database.HashGetAsync(BasketKey, id);

        return data.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize(
                Encoding.UTF8.GetString(data!),
                BasketSerializationContext.Default.CustomerBasket
            );
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        var database = await GetDatabaseAsync();
        var id = basket.Id;
        var json = JsonSerializer.Serialize(
            basket,
            BasketSerializationContext.Default.CustomerBasket
        );

        var created = await database.HashSetAsync(BasketKey, id, json);

        if (created)
        {
            return await GetBasketAsync(basket.Id!);
        }

        logger.LogError("[{Repository}] Failed to update basket", nameof(BasketRepository));
        return null;
    }

    public async Task<bool> DeleteBasketAsync(string id)
    {
        var database = await GetDatabaseAsync();
        return await database.HashDeleteAsync(BasketKey, id);
    }

    private async Task<IDatabase> GetDatabaseAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            return redis.GetDatabase();
        }
        finally
        {
            _connectionLock.Release();
        }
    }
}

[JsonSerializable(typeof(CustomerBasket))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
public partial class BasketSerializationContext : JsonSerializerContext;

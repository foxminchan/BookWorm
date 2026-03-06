using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace BookWorm.Basket.Infrastructure.Repositories;

public sealed class BasketRepository(ILogger<BasketRepository> logger, IConnectionMultiplexer redis)
    : IBasketRepository
{
    public async Task<CustomerBasket?> GetBasketAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id
    )
    {
        var database = redis.GetDatabase();
        var data = await database.StringGetAsync(GetBasketKey(id));

        return data.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize(
                Encoding.UTF8.GetString(data!),
                BasketSerializationContext.Default.CustomerBasket
            );
    }

    public async Task<CustomerBasket?> CreateOrUpdateBasketAsync(CustomerBasket basket)
    {
        var database = redis.GetDatabase();
        var json = JsonSerializer.Serialize(
            basket,
            BasketSerializationContext.Default.CustomerBasket
        );

        var created = await database.StringSetAsync(GetBasketKey(basket.Id), json);

        if (created)
        {
            return await GetBasketAsync(basket.Id);
        }

        logger.LogError("[{Repository}] Failed to update basket", nameof(BasketRepository));
        return null;
    }

    public async Task<bool> DeleteBasketAsync(
        [StringSyntax(StringSyntaxAttribute.GuidFormat)] string id
    )
    {
        var database = redis.GetDatabase();
        return await database.KeyDeleteAsync(GetBasketKey(id));
    }

    private static RedisKey GetBasketKey(string userId)
    {
        return $"basket:{userId}";
    }
}

[JsonSerializable(typeof(CustomerBasket))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class BasketSerializationContext : JsonSerializerContext;

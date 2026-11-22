using System.Text.Json;
using StackExchange.Redis;

namespace BookWorm.Ordering.Infrastructure.Idempotency;

internal sealed class RequestManager(IConnectionMultiplexer redis) : IRequestManager
{
    private const int DefaultExpirationTime = 3600;

    public async Task<bool> IsExistAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = redis.GetDatabase();
        return await database.KeyExistsAsync(idempotencyKey);
    }

    public async Task CreateAsync(
        ClientRequest clientRequest,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = redis.GetDatabase();

        var json = JsonSerializer.Serialize(
            clientRequest,
            IdempotencySerializationContext.Default.ClientRequest
        );

        await database.StringSetAsync(
            clientRequest.Id,
            json,
            TimeSpan.FromSeconds(DefaultExpirationTime)
        );
    }

    public async Task<bool> TryCreateAsync(
        ClientRequest clientRequest,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = redis.GetDatabase();

        var json = JsonSerializer.Serialize(
            clientRequest,
            IdempotencySerializationContext.Default.ClientRequest
        );

        return await database.StringSetAsync(
            clientRequest.Id,
            json,
            TimeSpan.FromSeconds(DefaultExpirationTime),
            When.NotExists
        );
    }

    public async Task DeleteAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var database = redis.GetDatabase();
        await database.KeyDeleteAsync(idempotencyKey);
    }
}

[JsonSerializable(typeof(ClientRequest))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class IdempotencySerializationContext : JsonSerializerContext;

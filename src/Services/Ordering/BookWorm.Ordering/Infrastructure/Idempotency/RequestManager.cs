using System.Text.Json;
using StackExchange.Redis;

namespace BookWorm.Ordering.Infrastructure.Idempotency;

internal sealed class RequestManager(IConnectionMultiplexer redis) : IRequestManager
{
    private const int DefaultExpirationTime = 3600;

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public async Task<bool> IsExistAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    )
    {
        var database = await GetDatabaseAsync();
        return await database.KeyExistsAsync(idempotencyKey);
    }

    public async Task CreateAsync(
        ClientRequest clientRequest,
        CancellationToken cancellationToken = default
    )
    {
        var database = await GetDatabaseAsync();

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

[JsonSerializable(typeof(ClientRequest))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class IdempotencySerializationContext : JsonSerializerContext;

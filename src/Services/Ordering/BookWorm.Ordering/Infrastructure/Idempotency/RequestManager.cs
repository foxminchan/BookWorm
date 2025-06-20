﻿using System.Text.Json;
using System.Text.RegularExpressions;
using StackExchange.Redis;

namespace BookWorm.Ordering.Infrastructure.Idempotency;

public sealed partial class RequestManager(
    ILogger<RequestManager> logger,
    IConnectionMultiplexer redis
) : IRequestManager
{
    private const int DefaultExpirationTime = 3600;

    private static readonly Regex _sanitizeLoggingRegex = SanitizeLoggingRegex();

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

        var created = await database.StringSetAsync(
            clientRequest.Id,
            json,
            TimeSpan.FromSeconds(DefaultExpirationTime)
        );

        if (!created)
        {
            var sanitizedId = _sanitizeLoggingRegex.Replace(clientRequest.Id, string.Empty);
            logger.LogError(
                "[{RequestManager}] Failed to create request for {IdempotencyKey}",
                nameof(RequestManager),
                sanitizedId
            );
        }
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

    [GeneratedRegex(@"[\r\n<>(){}[\]\\/&*#$%^|;:]", RegexOptions.Compiled)]
    private static partial Regex SanitizeLoggingRegex();
}

[JsonSerializable(typeof(ClientRequest))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
public partial class IdempotencySerializationContext : JsonSerializerContext;

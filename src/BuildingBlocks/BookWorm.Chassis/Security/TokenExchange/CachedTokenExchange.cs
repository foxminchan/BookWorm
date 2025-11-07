using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Hybrid;

namespace BookWorm.Chassis.Security.TokenExchange;

internal sealed class CachedTokenExchange(
    ITokenExchange inner,
    HybridCache cache,
    ClaimsPrincipal principal
) : ITokenExchange
{
    private const string CacheKeyPrefix = "token:";
    private static readonly string[] _cacheTags = ["token", "security"];

    public async Task<TokenExchangeResult> ExchangeAsync(
        string subjectToken,
        string? audience = null,
        string? scope = null,
        CancellationToken cancellationToken = default
    )
    {
        var userId =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException(
                "No HTTP context or user available for token exchange caching"
            );

        var cacheKey = GenerateCacheKey(userId, audience);

        var result = await cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                var exchangedToken = await inner.ExchangeAsync(
                    subjectToken,
                    audience,
                    scope,
                    cancel
                );

                return exchangedToken;
            },
            options: new()
            {
                Expiration = CalculateExpiration(),
                LocalCacheExpiration = CalculateExpiration(),
            },
            tags: _cacheTags,
            cancellationToken: cancellationToken
        );

        var actualExpiration = CalculateExpiration(result.ExpiresIn);

        await cache.SetAsync(
            cacheKey,
            result,
            new() { Expiration = actualExpiration, LocalCacheExpiration = actualExpiration },
            tags: _cacheTags,
            cancellationToken: cancellationToken
        );

        return result ?? throw new InvalidOperationException("Token exchange returned null result");
    }

    private static string GenerateCacheKey(string userId, string? audience)
    {
        var keyBuilder = new StringBuilder(CacheKeyPrefix).Append(userId);

        if (!string.IsNullOrWhiteSpace(audience))
        {
            keyBuilder.Append(':').Append(audience);
        }

        return keyBuilder.ToString();
    }

    private static TimeSpan CalculateExpiration(int expiresInSeconds = 0)
    {
        if (expiresInSeconds <= 0)
        {
            return TimeSpan.FromMinutes(1);
        }

        // Use 90% of token lifetime to ensure we refresh before expiration
        const double safetyMargin = 0.9;
        var safeExpirationSeconds = (int)(expiresInSeconds * safetyMargin);

        return TimeSpan.FromSeconds(Math.Max(safeExpirationSeconds, 60));
    }
}

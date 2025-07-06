using BookWorm.ServiceDefaults.Kestrel;
using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Keycloak;

public sealed class KeycloakUrls(ServiceEndpointResolver serviceEndpointResolver) : IKeycloakUrls
{
    private readonly string _realmName =
        Environment.GetEnvironmentVariable("REALM") ?? nameof(BookWorm).ToLowerInvariant();

    public async Task<string> GetAccountConsoleUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return await GetRealmUrlAsync(serviceName, "account", cancellationToken);
    }

    public async Task<string> GetTokenUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return await GetRealmUrlAsync(
            serviceName,
            "protocol/openid-connect/token",
            cancellationToken
        );
    }

    public async Task<string> GetAuthorizationUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return await GetRealmUrlAsync(
            serviceName,
            "protocol/openid-connect/auth",
            cancellationToken
        );
    }

    /// <summary>
    /// Asynchronously resolves the full URL for a Keycloak realm endpoint, optionally appending a specific path segment.
    /// </summary>
    /// <param name="serviceName">The name of the Keycloak service to target.</param>
    /// <param name="path">An optional path segment to append after the realm name.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The resolved URL as a string for the specified Keycloak realm endpoint.</returns>
    private async Task<string> GetRealmUrlAsync(
        string serviceName,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var baseUri = $"{Protocol.HttpOrHttps}://{serviceName}/realms/{_realmName}";
        var realmPath = string.IsNullOrWhiteSpace(path) ? null : $"/{path}";
        return await serviceEndpointResolver.ResolveServiceEndpointUrl(
            baseUri,
            realmPath,
            cancellationToken
        );
    }
}

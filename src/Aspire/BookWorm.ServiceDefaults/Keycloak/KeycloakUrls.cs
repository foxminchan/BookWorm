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

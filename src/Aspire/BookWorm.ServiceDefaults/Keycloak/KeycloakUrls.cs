using BookWorm.ServiceDefaults.Kestrel;
using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Keycloak;

public sealed class KeycloakUrls(
    ServiceEndpointResolver serviceEndpointResolver,
    IdentityOptions identity
) : IKeycloakUrls
{
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

    public Task<string> GetIntrospectionUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return GetRealmUrlAsync(
            serviceName,
            "protocol/openid-connect/token/introspect",
            cancellationToken
        );
    }

    public Task<string> GetUserInfoUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return GetRealmUrlAsync(serviceName, "protocol/openid-connect/userinfo", cancellationToken);
    }

    public Task<string> GetEndSessionUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return GetRealmUrlAsync(serviceName, "protocol/openid-connect/logout", cancellationToken);
    }

    public Task<string> GetRegistrationUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return GetRealmUrlAsync(
            serviceName,
            "protocol/openid-connect/registrations",
            cancellationToken
        );
    }

    public Task<string> GetMetadataUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return GetRealmUrlAsync(serviceName, ".well-known/openid-configuration", cancellationToken);
    }

    private async Task<string> GetRealmUrlAsync(
        string serviceName,
        string? path = null,
        CancellationToken cancellationToken = default
    )
    {
        var baseUri = $"{Protocol.HttpOrHttps}://{serviceName}/realms/{identity.Realm}";
        var realmPath = string.IsNullOrWhiteSpace(path) ? null : $"/{path}";
        return await serviceEndpointResolver.ResolveServiceEndpointUrl(
            baseUri,
            realmPath,
            cancellationToken: cancellationToken
        );
    }
}

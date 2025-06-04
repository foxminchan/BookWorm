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
        var serviceLookupName = $"https+http://{serviceName}";
        var serviceAddresses = (
            await serviceEndpointResolver.GetEndpointsAsync(serviceLookupName, cancellationToken)
        )
            .Endpoints.Select(e => e.EndPoint.ToString())
            .ToList();

        var firstHttpsEndpointUrl = serviceAddresses.FirstOrDefault(e =>
            e?.StartsWith("https://") == true
        );
        var endpointUrl =
            (
                firstHttpsEndpointUrl
                ?? serviceAddresses.FirstOrDefault(e => e?.StartsWith("http://") == true)
            )
            ?? throw new InvalidOperationException(
                $"No HTTP(S) endpoints found for service '{serviceName}'."
            );

        var uriBuilder = new UriBuilder(endpointUrl) { Path = $"/realms/{_realmName}" };

        if (!string.IsNullOrWhiteSpace(path))
        {
            uriBuilder.Path += $"/{path}";
        }

        return uriBuilder.Uri.ToString();
    }
}

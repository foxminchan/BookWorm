using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Keycloak;

/// <summary>
///     A service that provides URLs for Keycloak. Register as a singleton.
/// </summary>
public sealed class KeycloakUrls(ServiceEndpointResolver serviceEndpointResolver) : IKeycloakUrls
{
    private const string RealmName = "bookworm";

    /// <summary>
    ///     Gets the URL for the Keycloak Account Console that allows end users to manage their own profile information.<br />
    ///     This URL will be rendered in the browser so users can click on it so is resolved to real addresses via service
    ///     discovery.
    /// </summary>
    /// <remarks>
    ///     See
    ///     <see href="https://www.keycloak.org/docs/25.0.2/server_admin/#_account-service">https://www.keycloak.org/docs/25.0.2/server_admin/#_account-service</see>
    ///     for more information on the Keycloak Account Console.
    /// </remarks>
    public async Task<string> GetAccountConsoleUrlAsync(
        string serviceName,
        CancellationToken cancellationToken = default
    )
    {
        return await GetRealmUrlAsync(serviceName, "account", cancellationToken);
    }

    /// <summary>
    ///     Returns the URL for the Keycloak realm with the given service name and scheme.<br />
    ///     The realm name is read from configuration using the key <c>Authentication:Keycloak:Realm</c>.
    /// </summary>
    /// <remarks>
    ///     Note the URL is <strong>not</strong> resolved to real addresses via service discovery.
    /// </remarks>
    public static string GetRealmUrl(string serviceName, string scheme = "https")
    {
        return $"{scheme}://{serviceName}/realms/{RealmName}";
    }

    /// <summary>
    ///     Gets the URL for the Keycloak server with the given service name.
    /// </summary>
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

        var uriBuilder = new UriBuilder(endpointUrl) { Path = $"/realms/{RealmName}" };

        if (!string.IsNullOrWhiteSpace(path))
        {
            uriBuilder.Path += $"/{path}";
        }

        return uriBuilder.Uri.ToString();
    }
}

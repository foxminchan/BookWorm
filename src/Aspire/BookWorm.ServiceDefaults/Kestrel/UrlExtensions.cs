using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class UrlExtensions
{
    public static async Task<string> ResolveServiceEndpointUrl(
        this ServiceEndpointResolver serviceEndpointResolver,
        string uri,
        string? additionalPath = null,
        string? preferredProtocol = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(uri));
        }

        var parsedUri = new Uri(uri);
        var serviceName = $"{parsedUri.Scheme}://{parsedUri.Host}";

        var endpoints = await serviceEndpointResolver.GetEndpointsAsync(
            serviceName,
            cancellationToken
        );
        var serviceAddresses = endpoints.Endpoints.Select(e => e.EndPoint.ToString()).ToArray();

        var baseEndpoint = preferredProtocol switch
        {
            _ when preferredProtocol == Protocols.Http => serviceAddresses.FirstOrDefault(e =>
                e?.StartsWith($"{Protocols.Http}://") == true
            )
                ?? serviceAddresses.FirstOrDefault(e =>
                    e?.StartsWith($"{Protocols.Https}://") == true
                ),
            _ when preferredProtocol == Protocols.Https => serviceAddresses.FirstOrDefault(e =>
                e?.StartsWith($"{Protocols.Https}://") == true
            )
                ?? serviceAddresses.FirstOrDefault(e =>
                    e?.StartsWith($"{Protocols.Http}://") == true
                ),
            _ => serviceAddresses.FirstOrDefault(e =>
                e?.StartsWith($"{Protocols.Https}://") == true
            )
                ?? serviceAddresses.FirstOrDefault(e =>
                    e?.StartsWith($"{Protocols.Http}://") == true
                ),
        };

        return baseEndpoint is null
            ? throw new InvalidOperationException(
                $"No HTTP(S) endpoints found for service '{serviceName}'."
            )
            : $"{baseEndpoint.TrimEnd('/')}{parsedUri.PathAndQuery}{additionalPath}";
    }
}

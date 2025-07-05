using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class UrlExtensions
{
    public static async Task<string> ResolveServiceEndpointUrl(
        this ServiceEndpointResolver serviceEndpointResolver,
        string uri,
        string? additionalPath = null,
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

        var baseEndpoint =
            serviceAddresses.FirstOrDefault(e => e?.StartsWith($"{Protocol.Https}://") == true)
            ?? serviceAddresses.FirstOrDefault(e => e?.StartsWith($"{Protocol.Http}://") == true)
            ?? throw new InvalidOperationException(
                $"No HTTP(S) endpoints found for service '{serviceName}'."
            );

        // Reconstruct the full URL with the original path
        return $"{baseEndpoint.TrimEnd('/')}{parsedUri.PathAndQuery}{additionalPath}";
    }
}

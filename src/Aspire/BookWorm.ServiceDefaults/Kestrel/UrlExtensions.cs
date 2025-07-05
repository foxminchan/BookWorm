using Microsoft.Extensions.ServiceDiscovery;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class UrlExtensions
{
    public static async Task<string> ResolveServiceEndpointUrl(
        this ServiceEndpointResolver serviceEndpointResolver,
        string uri,
        string? additionalPath = null
    )
    {
        var parsedUri = new Uri(uri);
        var serviceName = $"{parsedUri.Scheme}://{parsedUri.Host}";

        var endpoints = await serviceEndpointResolver.GetEndpointsAsync(
            serviceName,
            CancellationToken.None
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

using BookWorm.Constants.Core;

namespace BookWorm.Chassis.Utilities;

public static class ServiceDiscoveryUtilities
{
    public static string? GetServiceEndpoint(string serviceName, string endpointName, int index = 0)
    {
        return Environment.GetEnvironmentVariable(
            $"services__{serviceName}__{endpointName}__{index}"
        );
    }

    public static string? GetServiceEndpoint(string serviceName, int index = 0)
    {
        return GetServiceEndpoint(serviceName, Http.Schemes.Https, index)
            ?? GetServiceEndpoint(serviceName, Http.Schemes.Http, index);
    }

    public static string GetRequiredServiceEndpoint(string serviceName, int index = 0)
    {
        var endpoint = GetServiceEndpoint(serviceName, index);
        return string.IsNullOrEmpty(endpoint)
            ? throw new InvalidOperationException(
                $"Service endpoint for '{serviceName}' not found in environment variables."
            )
            : endpoint;
    }
}

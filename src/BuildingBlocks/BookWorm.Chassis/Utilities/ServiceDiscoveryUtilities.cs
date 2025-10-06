using BookWorm.Constants.Aspire;

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
        return Environment.GetEnvironmentVariable(
                $"services__{serviceName}__{Protocols.Https}__{index}"
            )
            ?? Environment.GetEnvironmentVariable(
                $"services__{serviceName}__{Protocols.Http}__{index}"
            );
    }
}

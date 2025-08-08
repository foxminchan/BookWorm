namespace BookWorm.ServiceDefaults.Configuration;

public static class ServiceDiscoveryUtilities
{
    public static string? GetServiceEndpoint(string serviceName, string endpointName, int index = 0)
    {
        return Environment.GetEnvironmentVariable(
            $"services__{serviceName}__{endpointName}__{index}"
        );
    }
}

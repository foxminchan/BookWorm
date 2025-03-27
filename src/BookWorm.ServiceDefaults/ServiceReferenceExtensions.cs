using Grpc.Health.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookWorm.ServiceDefaults;

public static class ServiceReferenceExtensions
{
    public static IHttpClientBuilder AddGrpcServiceReference<TClient>(
        this IServiceCollection services,
        string address,
        HealthStatus failureStatus
    )
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        var uri = new Uri(address);
        var builder = services.AddGrpcClient<TClient>(o => o.Address = uri);

        builder.AddStandardResilienceHandler();

        AddGrpcHealthChecks(services, uri, $"{typeof(TClient).Name}-health", failureStatus);

        return builder;
    }

    private static void AddGrpcHealthChecks(
        IServiceCollection services,
        Uri uri,
        string healthCheckName,
        HealthStatus failureStatus = default
    )
    {
        services
            .AddGrpcClient<Health.HealthClient>(o => o.Address = uri)
            .AddStandardResilienceHandler();
        services.AddHealthChecks().AddCheck<GrpcServiceHealthCheck>(healthCheckName, failureStatus);
    }

    private sealed class GrpcServiceHealthCheck(Health.HealthClient healthClient) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            var response = await healthClient.CheckAsync(
                new(),
                cancellationToken: cancellationToken
            );

            return response.Status switch
            {
                HealthCheckResponse.Types.ServingStatus.Serving => HealthCheckResult.Healthy(),
                _ => HealthCheckResult.Unhealthy(),
            };
        }
    }
}

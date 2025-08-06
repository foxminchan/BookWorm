using Grpc.Health.V1;
using Refit;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class ServiceReferenceExtensions
{
    private static readonly string _healthCheckName = nameof(Health).ToLowerInvariant();

    public static IHttpClientBuilder AddGrpcServiceReference<TClient>(
        this IServiceCollection services,
        string address,
        HealthStatus failureStatus
    )
        where TClient : class
    {
        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        var uri = new Uri(address);
        var builder = services.AddGrpcClient<TClient>(o => o.Address = uri);

        builder.AddStandardResilienceHandler();

        AddGrpcHealthChecks(
            services,
            uri,
            $"{typeof(TClient).Name}-{_healthCheckName}",
            failureStatus
        );

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

    public static void AddHttpServiceReference(
        this IServiceCollection services,
        string name,
        string address,
        HealthStatus failureStatus,
        string? healthRelativePath = null
    )
    {
        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        if (
            !string.IsNullOrEmpty(healthRelativePath)
            && !Uri.IsWellFormedUriString(healthRelativePath, UriKind.Relative)
        )
        {
            throw new ArgumentException(
                "Health check path must be a valid relative URI.",
                nameof(healthRelativePath)
            );
        }

        var uri = new Uri(address);

        services.AddHttpClient(name, c => c.BaseAddress = uri);

        services
            .AddHealthChecks()
            .AddUrlGroup(
                new Uri(uri, healthRelativePath ?? _healthCheckName),
                name,
                failureStatus,
                configurePrimaryHttpMessageHandler: s =>
                    s.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler()
            );
    }

    public static void AddHttpServiceReference<TClient>(
        this IServiceCollection services,
        string address,
        HealthStatus failureStatus,
        string? healthRelativePath = null
    )
        where TClient : class
    {
        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            throw new ArgumentException("Address must be a valid absolute URI.", nameof(address));
        }

        if (
            !string.IsNullOrEmpty(healthRelativePath)
            && !Uri.IsWellFormedUriString(healthRelativePath, UriKind.Relative)
        )
        {
            throw new ArgumentException(
                "Health check path must be a valid relative URI.",
                nameof(healthRelativePath)
            );
        }

        var uri = new Uri(address);

        services.AddRefitClient<TClient>().ConfigureHttpClient(c => c.BaseAddress = uri);

        services
            .AddHealthChecks()
            .AddUrlGroup(
                new Uri(uri, healthRelativePath ?? _healthCheckName),
                $"{typeof(TClient).Name}-{_healthCheckName}",
                failureStatus,
                configurePrimaryHttpMessageHandler: s =>
                    s.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler()
            );
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

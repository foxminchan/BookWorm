using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Http.Resilience;
using Polly.Timeout;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class ServiceReferenceExtensions
{
    private static readonly string _healthCheckName = nameof(Health).ToLowerInvariant();

    private static void AddGrpcHealthChecks(
        IServiceCollection services,
        Uri uri,
        string healthCheckName,
        HealthStatus failureStatus = default
    )
    {
        services
            .AddGrpcClient<Health.HealthClient>(o => o.Address = uri)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    EnableMultipleHttp2Connections = true,
                }
            )
            .AddStandardResilienceHandler()
            .UseBudgetedRetries();
        services.AddHealthChecks().AddCheck<GrpcServiceHealthCheck>(healthCheckName, failureStatus);
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds a typed gRPC client reference with resilience and health check registration.
        /// </summary>
        /// <typeparam name="TClient">
        ///     The typed gRPC client contract.
        /// </typeparam>
        /// <param name="address">
        ///     The absolute URI of the downstream gRPC service.
        /// </param>
        /// <param name="failureStatus">
        ///     One of the enumeration values that specifies the health status reported when the health probe fails.
        /// </param>
        /// <param name="clientName">
        ///     Optional name for a separately configured gRPC client.
        /// </param>
        /// <returns>
        ///     An HTTP client builder for the registered gRPC client.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Address is not a valid absolute URI.
        /// </exception>
        public IHttpClientBuilder AddGrpcServiceReference<TClient>(
            string address,
            HealthStatus failureStatus,
            string? clientName = null
        )
            where TClient : class
        {
            if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
            {
                throw new ArgumentException(
                    "Address must be a valid absolute URI.",
                    nameof(address)
                );
            }

            var uri = new Uri(address);
            var builder = clientName is null
                ? services.AddGrpcClient<TClient>(o => o.Address = uri)
                : services.AddGrpcClient<TClient>(clientName, o => o.Address = uri);

            builder
                .EnableCallContextPropagation(o => o.SuppressContextNotFoundErrors = true)
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new SocketsHttpHandler
                    {
                        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                        EnableMultipleHttp2Connections = true,
                    }
                );

            builder
                .AddStandardResilienceHandler(options =>
                {
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                })
                .UseBudgetedRetries();

            AddGrpcHealthChecks(
                services,
                uri,
                $"{clientName ?? typeof(TClient).Name}-{_healthCheckName}",
                failureStatus
            );

            return builder;
        }
    }

    private sealed class GrpcServiceHealthCheck(Health.HealthClient healthClient) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default
        )
        {
            try
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
            catch (RpcException exception)
                when (exception.StatusCode is StatusCode.Cancelled or StatusCode.DeadlineExceeded)
            {
                return HealthCheckResult.Unhealthy(exception.Status.Detail, exception);
            }
            catch (TimeoutRejectedException exception)
            {
                return HealthCheckResult.Unhealthy(exception.Message, exception);
            }
        }
    }
}

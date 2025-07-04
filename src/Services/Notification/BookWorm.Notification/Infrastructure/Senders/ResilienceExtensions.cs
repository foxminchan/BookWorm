using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace BookWorm.Notification.Infrastructure.Senders;

public static class ResilienceExtensions
{
    public static void AddMailResiliencePipeline(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.Configure<CircuitBreakerStrategyOptions>(
            nameof(CircuitBreakerStrategyOptions),
            builder.Configuration.GetSection(nameof(CircuitBreakerStrategyOptions))
        );

        services.PostConfigure<CircuitBreakerStrategyOptions>(
            nameof(CircuitBreakerStrategyOptions),
            options => options.ShouldHandle = new PredicateBuilder().Handle<Exception>()
        );

        services.Configure<RetryStrategyOptions>(
            nameof(RetryStrategyOptions),
            builder.Configuration.GetSection(nameof(RetryStrategyOptions))
        );

        services.PostConfigure<RetryStrategyOptions>(
            nameof(RetryStrategyOptions),
            options =>
            {
                options.ShouldHandle = new PredicateBuilder().Handle<Exception>();
                options.BackoffType = DelayBackoffType.Exponential;
                options.UseJitter = true;
            }
        );

        services.Configure<TimeoutStrategyOptions>(
            nameof(TimeoutStrategyOptions),
            builder.Configuration.GetSection(nameof(TimeoutStrategyOptions))
        );

        services.PostConfigure<TimeoutStrategyOptions>(
            nameof(TimeoutStrategyOptions),
            options =>
            {
                options.Timeout = TimeSpan.FromSeconds(30);
                options.TimeoutGenerator = null;
                options.OnTimeout = null;
            }
        );

        services.AddResiliencePipeline(
            nameof(Notification),
            (pipelineBuilder, context) =>
            {
                var provider = context.ServiceProvider;
                var circuitBreakerOptions = provider
                    .GetRequiredService<IOptionsMonitor<CircuitBreakerStrategyOptions>>()
                    .Get(nameof(CircuitBreakerStrategyOptions));
                var retryOptions = provider
                    .GetRequiredService<IOptionsMonitor<RetryStrategyOptions>>()
                    .Get(nameof(RetryStrategyOptions));
                var timeoutOptions = provider
                    .GetRequiredService<IOptionsMonitor<TimeoutStrategyOptions>>()
                    .Get(nameof(TimeoutStrategyOptions));

                pipelineBuilder.AddCircuitBreaker(circuitBreakerOptions);
                pipelineBuilder.AddRetry(retryOptions);
                pipelineBuilder.AddTimeout(timeoutOptions);
            }
        );
    }
}

using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BookWorm.Chassis.Utilities.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class RateLimiterExtensions
{
    private const string PerUserPolicy = "PerUserRateLimit";

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers and configures API rate limiting for the current host.
        /// </summary>
        /// <remarks>
        ///     This method configures:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 A global fixed-window limiter for all requests.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 A per-user token-bucket policy identified by <c>PerUserRateLimit</c>.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 A unified rejection behavior that returns HTTP <c>429</c> responses.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public void AddRateLimiting()
        {
            var services = builder.Services;

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Registers ASP.NET Core rate limiting services.
            services.AddRateLimiter();

            // Configures the global fixed-window limiter options.
            builder.Configure<FixedWindowRateLimiterOptions>(
                $"{nameof(RateLimiter)}:{nameof(FixedWindowRateLimiter)}",
                configure: options =>
                {
                    options.AutoReplenishment = true;
                    options.PermitLimit = 30;
                    options.QueueLimit = 0;
                    options.Window = TimeSpan.FromMinutes(1);
                }
            );

            // Configures the per-user token bucket limiter options.
            builder.Configure<TokenBucketRateLimiterOptions>(
                $"{nameof(RateLimiter)}:{nameof(TokenBucketRateLimiter)}",
                configure: options =>
                {
                    options.AutoReplenishment = true;
                    options.TokenLimit = 30;
                    options.TokensPerPeriod = 20;
                    options.QueueLimit = 5;
                    options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                }
            );

            // Configures server-side load shedding via concurrency limiter options.
            builder.Configure<ConcurrencyLimiterOptions>(
                $"{nameof(RateLimiter)}:{nameof(ConcurrencyLimiter)}",
                configure: options =>
                {
                    options.PermitLimit = 100;
                    options.QueueLimit = 0;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                }
            );

            // Applies rate limiter pipeline behavior and policy wiring.
            services
                .AddOptions<RateLimiterOptions>()
                .Configure(
                    (
                        RateLimiterOptions options,
                        IOptionsMonitor<TokenBucketRateLimiterOptions> userRateLimitingOptions,
                        IOptionsMonitor<FixedWindowRateLimiterOptions> windowRateLimiterOptions,
                        IOptionsMonitor<ConcurrencyLimiterOptions> concurrencyLimiterOptions
                    ) =>
                    {
                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                        options.AddRejectBehavior();

                        options.AddDefaultLimiter(
                            windowRateLimiterOptions,
                            concurrencyLimiterOptions
                        );

                        options.AddUserRateLimiter(userRateLimitingOptions);
                    }
                );
        }
    }

    extension(IEndpointConventionBuilder builder)
    {
        /// <summary>
        ///     Applies the per-user rate limiting policy to the endpoint.
        /// </summary>
        /// <returns>The endpoint convention builder with the rate limit policy applied.</returns>
        public IEndpointConventionBuilder RequirePerUserRateLimit()
        {
            return builder.RequireRateLimiting(PerUserPolicy);
        }
    }

    extension(RateLimiterOptions option)
    {
        private void AddDefaultLimiter(
            IOptionsMonitor<FixedWindowRateLimiterOptions> fixedWindowRateLimiterOptions,
            IOptionsMonitor<ConcurrencyLimiterOptions> concurrencyLimiterOptions
        )
        {
            var fixedWindowLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(ResolveClientPartitionKey(context), _ =>
                    fixedWindowRateLimiterOptions.CurrentValue
                )
            );

            var loadSheddingLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetConcurrencyLimiter("load-shedding", _ =>
                    concurrencyLimiterOptions.CurrentValue
                )
            );

            option.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                loadSheddingLimiter,
                fixedWindowLimiter
            );
        }

        private void AddUserRateLimiter(
            IOptionsMonitor<TokenBucketRateLimiterOptions> userRateLimitingOptions
        )
        {
            option.AddPolicy(PerUserPolicy, context =>
            {
                var partitionKey = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? ResolveClientPartitionKey(context);

                return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ =>
                    userRateLimitingOptions.CurrentValue
                );
            });
        }

        private void AddRejectBehavior()
        {
            option.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var serviceProvider = httpContext.RequestServices;

                var detail = "The server is currently shedding load. Please try again shortly.";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                        .ToString(CultureInfo.InvariantCulture);

                    httpContext.Response.Headers.RetryAfter = retryAfterSeconds;
                    detail =
                        $"You have exceeded the rate limit. Try again in {retryAfterSeconds} seconds.";
                }

                var problemDetailsFactory =
                    serviceProvider.GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = problemDetailsFactory.CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Rate limit exceeded",
                    detail
                );

                await httpContext.Response.WriteAsJsonAsync(
                    problemDetails,
                    RateLimiterSerializationContext.Default.ProblemDetails,
                    cancellationToken: cancellationToken
                );
            };
        }
    }

    private static string ResolveClientPartitionKey(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString()
            ?? context.Request.Headers.Host.ToString();
    }
}

[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class RateLimiterSerializationContext : JsonSerializerContext;

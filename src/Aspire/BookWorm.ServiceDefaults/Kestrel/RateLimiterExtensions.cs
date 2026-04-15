using System.Globalization;
using System.Threading.RateLimiting;
using BookWorm.Chassis.Utilities.Configurations;
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

            // Applies rate limiter pipeline behavior and policy wiring.
            services
                .AddOptions<RateLimiterOptions>()
                .Configure(
                    (
                        RateLimiterOptions options,
                        IOptionsMonitor<TokenBucketRateLimiterOptions> userRateLimitingOptions,
                        IOptionsMonitor<FixedWindowRateLimiterOptions> windowRateLimiterOptions
                    ) =>
                    {
                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                        options.AddRejectBehavior();

                        options.AddDefaultLimiter(windowRateLimiterOptions);

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
            IOptionsMonitor<FixedWindowRateLimiterOptions> fixedWindowRateLimiterOptions
        )
        {
            option.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString()
                        ?? context.Request.Headers.Host.ToString(),
                    _ => fixedWindowRateLimiterOptions.CurrentValue
                )
            );
        }

        private void AddUserRateLimiter(
            IOptionsMonitor<TokenBucketRateLimiterOptions> userRateLimitingOptions
        )
        {
            option.AddPolicy(
                PerUserPolicy,
                context =>
                {
                    var username = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (string.IsNullOrEmpty(username))
                    {
                        throw new UnauthorizedAccessException(
                            "User is not authenticated. Rate limiting requires an authenticated user."
                        );
                    }

                    return RateLimitPartition.GetTokenBucketLimiter(
                        username,
                        _ => userRateLimitingOptions.CurrentValue
                    );
                }
            );
        }

        private void AddRejectBehavior()
        {
            option.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var serviceProvider = httpContext.RequestServices;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var milliseconds = retryAfter.TotalMilliseconds.ToString(
                        CultureInfo.InvariantCulture
                    );

                    httpContext.Response.Headers.RetryAfter = milliseconds;

                    var problemDetailsFactory =
                        serviceProvider.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        httpContext,
                        StatusCodes.Status429TooManyRequests,
                        "Rate limit exceeded",
                        $"You have exceeded the rate limit. Try again in {milliseconds} ms."
                    );

                    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                }
            };
        }
    }
}

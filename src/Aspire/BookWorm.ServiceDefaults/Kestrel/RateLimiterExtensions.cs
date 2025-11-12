using System.Globalization;
using System.Threading.RateLimiting;
using BookWorm.Chassis.Utilities.Configuration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

namespace BookWorm.ServiceDefaults.Kestrel;

public static class RateLimiterExtensions
{
    private const string PerUserPolicy = "PerUserRateLimit";

    public static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter();

        services.Configure<FixedWindowRateLimiterOptions>(
            nameof(FixedWindowRateLimiter),
            configure: options =>
            {
                options.AutoReplenishment = true;
                options.PermitLimit = 30;
                options.QueueLimit = 5;
                options.Window = TimeSpan.FromMinutes(1);
            }
        );

        services.Configure<TokenBucketRateLimiterOptions>(
            nameof(TokenBucketRateLimiter),
            configure: options =>
            {
                options.AutoReplenishment = true;
                options.TokenLimit = 100;
                options.TokensPerPeriod = 100;
                options.QueueLimit = 100;
                options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            }
        );

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

    public static IEndpointConventionBuilder RequirePerUserRateLimit(
        this IEndpointConventionBuilder builder
    )
    {
        return builder.RequireRateLimiting(PerUserPolicy);
    }

    extension(RateLimiterOptions option)
    {
        private void AddDefaultLimiter(
            IOptionsMonitor<FixedWindowRateLimiterOptions> fixedWindowRateLimiterOptions
        )
        {
            option.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.GetClientIdentifier(),
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

    private static string GetClientIdentifier(this HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString()
            ?? context.Request.Headers.Host.ToString();
    }
}

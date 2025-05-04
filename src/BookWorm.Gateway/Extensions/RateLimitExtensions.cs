using System.Threading.RateLimiting;

namespace BookWorm.Gateway.Extensions;

public static class RateLimitExtensions
{
    public static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                    var problemDetailsFactory =
                        httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        httpContext,
                        StatusCodes.Status429TooManyRequests,
                        "Rate limit exceeded",
                        $"You have exceeded the rate limit. Try again in {retryAfter} minutes."
                    );

                    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

                    var logger = httpContext.RequestServices.GetRequiredService<
                        ILogger<RateLimiter>
                    >();

                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning(
                            "Rate limit exceeded for {Path} from {IpAddress} by User {User}",
                            httpContext.Request.Path,
                            httpContext.Connection.RemoteIpAddress,
                            httpContext.User.Identity?.Name
                        );
                    }
                }
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? httpContext.Request.Headers.Host.ToString(),
                        _ =>
                            new()
                            {
                                AutoReplenishment = true,
                                PermitLimit = 10,
                                QueueLimit = 0,
                                Window = TimeSpan.FromMinutes(1),
                            }
                    )
            );
        });
    }
}

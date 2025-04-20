using System.Threading.RateLimiting;
using BookWorm.ServiceDefaults.Auth;
using BookWorm.ServiceDefaults.Kestrel;
using BookWorm.ServiceDefaults.Keycloak;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace BookWorm.Gateway;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        const string proxySection = "ReverseProxy";

        builder.AddDefaultCors();

        builder.AddDefaultAuthentication().AddKeycloakClaimsTransformation();

        services.AddProblemDetails();

        services.AddHttpForwarderWithServiceDiscovery();

        services.AddRequestTimeouts(options =>
            options.AddPolicy("timeout-1-minute", TimeSpan.FromMinutes(1))
        );

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

        services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection(proxySection))
            .AddServiceDiscoveryDestinationResolver();
    }
}

using System.Threading.RateLimiting;
using BookWorm.ServiceDefaults.Auth;
using BookWorm.ServiceDefaults.Kestrel;
using BookWorm.ServiceDefaults.Keycloak;

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
            var window = TimeSpan.FromMinutes(1);
            const int permitLimit = 10;
            const int queueLimit = 0;

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.Headers.RetryAfter = window.Minutes.ToString();

                await httpContext.Response.WriteAsync(
                    "Rate limit exceeded. Try again in " + window.Minutes + " minutes.",
                    cancellationToken
                );

                var logger = httpContext.RequestServices.GetRequiredService<ILogger<RateLimiter>>();

                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        "Rate limit exceeded for {Path} from {IpAddress} by User {User}",
                        httpContext.Request.Path,
                        httpContext.Connection.RemoteIpAddress,
                        httpContext.User.Identity?.Name
                    );
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
                                PermitLimit = permitLimit,
                                QueueLimit = queueLimit,
                                Window = window,
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

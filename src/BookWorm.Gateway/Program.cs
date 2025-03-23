using System.Threading.RateLimiting;
using BookWorm.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

builder.AddServiceDefaults();

builder.AddDefaultAuthentication();

services.AddHttpForwarderWithServiceDiscovery();

services.AddRequestTimeouts(options =>
    options.AddPolicy("timeout-1-minute", TimeSpan.FromMinutes(1))
);

services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "fixed-by-ip",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new() { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }
            )
    );
});

services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseRequestTimeouts();

app.UseRateLimiter();

app.MapDefaultEndpoints();

app.MapReverseProxy();

app.Run();

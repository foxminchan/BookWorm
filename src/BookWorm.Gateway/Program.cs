using System.Threading.RateLimiting;
using BookWorm.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddDefaultAuthentication();

builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddRequestTimeouts(options =>
    options.AddPolicy("timeout-1-minute", TimeSpan.FromMinutes(1))
);

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(
        "fixed-by-ip",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString(),
                _ => new() { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) }
            )
    );
});

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseRequestTimeouts();

app.UseRateLimiter();

app.MapDefaultEndpoints();

app.MapReverseProxy();

app.Run();

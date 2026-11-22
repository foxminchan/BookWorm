var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseForwardedHeaders();

app.UseMcpCors();

app.UseRateLimiter();

app.MapMcp("/mcp").RequireAuthorization();

app.Run();

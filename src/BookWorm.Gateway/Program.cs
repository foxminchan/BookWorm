using BookWorm.Gateway.Extensions;
using BookWorm.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseRateLimiter();

app.UseRequestTimeouts();

app.MapReverseProxy();

app.Run();

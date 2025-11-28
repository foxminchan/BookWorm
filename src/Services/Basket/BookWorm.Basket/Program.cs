using BookWorm.Basket.Extensions;
using BookWorm.Basket.Grpc.Services.Basket;
using BookWorm.Basket.Infrastructure.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.Security.Keycloak;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseDefaultCors();

app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();

app.UseRateLimiter();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet, "baskets");

app.MapGrpcService<BasketService>();

app.MapGrpcHealthChecksService();

app.MapAgentsDiscovery();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.UseDevUI();

app.Run();

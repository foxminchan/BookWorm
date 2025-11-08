using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chat.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.ServiceDefaults;

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

app.UseOutputCache();

app.UseDefaultCors();

app.UseRequestTimeouts();

app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();

app.UseRateLimiter();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet, "chats");

app.MapAgentsDiscovery();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.UseDevUI();

app.Run();

using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Chat.Extensions;
using BookWorm.Chat.Orchestration;
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

app.UseSecurityHeaders();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseDefaultCors();

app.UseKeycloakTokenIntrospection();

app.UseRateLimiter();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(ApiVersions.V1)
    .ReportApiVersions()
    .Build();

app.MapEndpoints(apiVersionSet, "chats");

app.MapAgentsDiscovery();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.UseDevUI();

app.Run();

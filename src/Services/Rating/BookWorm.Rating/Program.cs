using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Constants.Core;
using BookWorm.Rating.Extensions;
using BookWorm.Rating.Infrastructure.Agents;

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

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(ApiVersions.V1)
    .ReportApiVersions()
    .Build();

app.MapEndpoints(apiVersionSet, "feedbacks");

app.MapAgentsDiscovery();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.UseDevUI();

app.Run();

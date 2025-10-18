using BookWorm.Chassis.AI.Extensions;
using BookWorm.Rating.Extensions;
using BookWorm.Rating.Infrastructure.Agents;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
using Microsoft.Agents.AI.Hosting.A2A.AspNetCore;

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

app.MapEndpoints(apiVersionSet, "feedbacks");

var ratingAgent = app.Services.GetRequiredKeyedService<AIAgent>(RatingAgent.Name);

app.MapA2A(
    new A2AHostAgent(ratingAgent, RatingAgent.AgentCard).TaskManager!,
    $"/a2a/{RatingAgent.Name}"
);

app.MapAgentDiscovery("/agents");

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();

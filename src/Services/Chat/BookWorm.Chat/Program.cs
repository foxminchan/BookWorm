using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.ServiceDefaults;
using Microsoft.Agents.AI.Hosting.A2A.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

app.MapA2A(SummarizeAgent.Name, "/a2a/summarize", SummarizeAgent.AgentCard);

app.MapAgentDiscovery("/agents");

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();

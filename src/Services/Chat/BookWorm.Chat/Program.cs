using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chat.Extensions;
using BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;
using BookWorm.ServiceDefaults;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.A2A;
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

var summarizeAgent = app.Services.GetRequiredKeyedService<AIAgent>(SummarizeAgent.Name);

app.MapA2A(
    new A2AHostAgent(summarizeAgent, SummarizeAgent.AgentCard).TaskManager!,
    $"/a2a/{SummarizeAgent.Name}"
);

var languageAgent = app.Services.GetRequiredKeyedService<AIAgent>(LanguageAgent.Name);

app.MapA2A(
    new A2AHostAgent(languageAgent, LanguageAgent.AgentCard).TaskManager!,
    $"/a2a/{LanguageAgent.Name}"
);

app.MapAgentDiscovery("/agents");

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();

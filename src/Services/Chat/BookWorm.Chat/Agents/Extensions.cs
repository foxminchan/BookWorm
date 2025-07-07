using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using SharpA2A.AspNetCore;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKeyedSingleton(
            nameof(BookAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                var mcpClient = sp.GetRequiredService<IMcpClient>();
                var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                return BookAgent
                    .CreateAgentWithPluginsAsync(kernel, mcpClient, resolver)
                    .GetAwaiter()
                    .GetResult();
            }
        );

        services.AddKeyedSingleton(
            nameof(LanguageAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return LanguageAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            nameof(SummarizeAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return SummarizeAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            nameof(SentimentAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return SentimentAgent.CreateAgent(kernel);
            }
        );
    }

    public static void MapHostSummarizeAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            nameof(SummarizeAgent)
        );

        var hostAgent = new A2AHostAgent(agent, SummarizeAgent.GetAgentCard());
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/summarize")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(SummarizeAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/summarize")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(SummarizeAgent));
    }

    public static void MapHostSentimentAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            nameof(SentimentAgent)
        );

        var hostAgent = new A2AHostAgent(agent, SentimentAgent.GetAgentCard());
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/sentiment")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(SentimentAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/sentiment")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(SentimentAgent));
    }
}

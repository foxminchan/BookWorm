using System.Diagnostics.CodeAnalysis;
using A2A;
using A2A.AspNetCore;
using BookWorm.Chassis.RAG.A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddA2AClient("RatingAgent", $"{Protocols.HttpOrHttps}://{Services.Rating}");

        services.AddKeyedSingleton(
            nameof(BookAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return BookAgent.CreateAgentAsync(kernel).GetAwaiter().GetResult();
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

        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
                tracing
                    .AddSource(TaskManager.ActivitySource.Name)
                    .AddSource(A2AJsonRpcProcessor.ActivitySource.Name)
            );
    }

    public static void MapHostSummarizeAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            nameof(SummarizeAgent)
        );

        var hostAgent = new A2AHostAgent(agent, SummarizeAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(nameof(SummarizeAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(nameof(SummarizeAgent));
    }
}

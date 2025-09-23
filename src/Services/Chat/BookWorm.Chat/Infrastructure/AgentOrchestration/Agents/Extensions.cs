using A2A;
using A2A.AspNetCore;
using BookWorm.Chassis.AI.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;

namespace BookWorm.Chat.Infrastructure.AgentOrchestration.Agents;

internal static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddA2AClient(
            Constants.Other.Agents.RatingAgent,
            $"{Protocols.HttpOrHttps}://{Services.Rating}"
        );

        services.AddKeyedSingleton(
            Constants.Other.Agents.BookAgent,
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return BookAgent.CreateAgentAsync(kernel).GetAwaiter().GetResult();
            }
        );

        services.AddKeyedSingleton(
            Constants.Other.Agents.LanguageAgent,
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return LanguageAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            Constants.Other.Agents.SummarizeAgent,
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return SummarizeAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            Constants.Other.Agents.SentimentAgent,
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
            Constants.Other.Agents.SummarizeAgent
        );

        var hostAgent = new A2AHostAgent(agent, SummarizeAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(Constants.Other.Agents.SummarizeAgent);

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(Constants.Other.Agents.SummarizeAgent);
    }
}

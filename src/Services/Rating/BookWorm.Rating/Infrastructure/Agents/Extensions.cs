using A2A;
using A2A.AspNetCore;
using BookWorm.Chassis.RAG.A2A;
using BookWorm.Chassis.RAG.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Infrastructure.Agents;

public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddEmbeddingGenerator();

        services.AddA2AClient(
            Constants.Other.Agents.SummarizeAgent,
            $"{Protocols.HttpOrHttps}://{Constants.Aspire.Services.Chatting}"
        );

        services.AddKeyedSingleton(
            Constants.Other.Agents.RatingAgent,
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return RatingAgent.CreateAgent(kernel);
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

    public static void MapHostRatingAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            Constants.Other.Agents.RatingAgent
        );

        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(Constants.Other.Agents.RatingAgent);

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(Constants.Other.Agents.RatingAgent);
    }
}

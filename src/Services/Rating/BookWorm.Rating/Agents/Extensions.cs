using A2A;
using A2A.AspNetCore;
using BookWorm.Chassis.RAG;
using BookWorm.Chassis.RAG.A2A;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Agents;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKernel();

        builder.AddSkTelemetry();

        builder.AddChatCompletion();

        builder.AddEmbeddingGenerator();

        services.AddA2AClient("SummarizeAgent", $"{Protocols.HttpOrHttps}://{Services.Chatting}");

        services.AddKeyedSingleton(
            nameof(RatingAgent),
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
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(nameof(RatingAgent));

        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/").WithTags(nameof(RatingAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/").WithTags(nameof(RatingAgent));
    }
}

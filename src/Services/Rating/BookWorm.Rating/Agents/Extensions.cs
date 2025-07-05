using BookWorm.Chassis.AI;
using BookWorm.Rating.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using SharpA2A.AspNetCore;

namespace BookWorm.Rating.Agents;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var kernel = services.AddKernel();

        builder.AddChatCompletion();
        builder.AddEmbeddingGenerator();
        kernel.Plugins.AddFromType<ReviewPlugin>();

        services.AddKeyedSingleton(
            nameof(RatingAgent),
            (serviceProvider, key) =>
                RatingAgent.CreateAgent(serviceProvider.GetRequiredService<Kernel>())
        );
    }

    public static void MapHostRatingAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(nameof(RatingAgent));
        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());
        app.MapA2A(hostAgent.TaskManager!, string.Empty);
    }
}

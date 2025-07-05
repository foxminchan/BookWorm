using BookWorm.Chassis.AI;
using BookWorm.Rating.Plugins;
using Microsoft.SemanticKernel;

namespace BookWorm.Rating.Agents;

public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var kernelBuilder = services.AddKernel();

        builder.AddChatCompletion();
        builder.AddEmbeddingGenerator();
        kernelBuilder.Plugins.AddFromType<ReviewPlugin>();

        var kernel = kernelBuilder.Build();

        services.AddKeyedSingleton(nameof(RatingAgent), () => RatingAgent.CreateAgent(kernel));
    }
}

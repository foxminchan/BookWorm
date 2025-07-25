using BookWorm.Chassis.AI;
using Microsoft.Extensions.ServiceDiscovery;
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

        services.AddKernel();
        builder.AddSkTelemetry();
        builder.AddChatCompletion();
        builder.AddEmbeddingGenerator();

        services.AddKeyedSingleton(
            nameof(RatingAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                return RatingAgent.CreateAgentAsync(kernel, resolver).GetAwaiter().GetResult();
            }
        );
    }

    public static void MapHostRatingAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(nameof(RatingAgent));

        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());

        app.MapA2A(hostAgent.TaskManager!, "/agents/rating").WithTags(nameof(RatingAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/agents/rating").WithTags(nameof(RatingAgent));
    }
}

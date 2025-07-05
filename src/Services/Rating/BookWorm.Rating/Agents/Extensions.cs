using BookWorm.Chassis.AI;
using BookWorm.Rating.Plugins;
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
        var kernelBuilder = services.AddKernel();

        builder.AddChatCompletion();
        builder.AddEmbeddingGenerator();

        var baseUri = $"{Protocol.HttpOrHttps}://{Application.Chatting}/agents";

        var sentimentPlugin = builder
            .ConnectRemoteAgent([$"{baseUri}/summarize", $"{baseUri}/sentiment"])
            .GetAwaiter()
            .GetResult();

        kernelBuilder.Plugins.AddFromType<ReviewPlugin>().Add(sentimentPlugin);

        services.AddKeyedSingleton(
            nameof(RatingAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return RatingAgent.CreateAgent(kernel);
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

    private static async Task<KernelPlugin> ConnectRemoteAgent(
        this IHostApplicationBuilder builder,
        string[] agentUris
    )
    {
        var serviceEndpointResolver = builder
            .Services.BuildServiceProvider()
            .GetRequiredService<ServiceEndpointResolver>();

        var endpointUrls = new List<string>();

        foreach (var uri in agentUris)
        {
            var resolvedUrl = await serviceEndpointResolver.ResolveServiceEndpointUrl(uri, "/");
            endpointUrls.Add(resolvedUrl);
        }

        var agents = await Task.WhenAll(endpointUrls.Select(uri => uri.CreateAgentAsync()));
        var agentFunctions = agents.Select(agent =>
            AgentKernelFunctionFactory.CreateFromAgent(agent)
        );

        return KernelPluginFactory.CreateFromFunctions("AgentPlugin", agentFunctions);
    }
}

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
    /// <summary>
    /// Configures and registers AI agent services, including local and remote plugins, for the host application.
    /// </summary>
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var kernelBuilder = services.AddKernel();

        builder.AddSkTelemetry();
        builder.AddChatCompletion();
        builder.AddEmbeddingGenerator();

        var sentimentPlugin = builder.GetRemoteAgentPlugin().GetAwaiter().GetResult();

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

    /// <summary>
    /// Maps HTTP and agent-to-agent endpoints for the RatingAgent to the application under "/agents/rating".
    /// </summary>
    public static void MapHostRatingAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(nameof(RatingAgent));
        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());
        app.MapA2A(hostAgent.TaskManager!, "/agents/rating").WithTags(nameof(RatingAgent));
        app.MapHttpA2A(hostAgent.TaskManager!, "/agents/rating").WithTags(nameof(RatingAgent));
    }

    /// <summary>
    /// Asynchronously retrieves a kernel plugin that aggregates remote agent functions for summarization and sentiment analysis.
    /// </summary>
    /// <returns>A <see cref="KernelPlugin"/> containing functions from the connected remote agents.</returns>
    private static async Task<KernelPlugin> GetRemoteAgentPlugin(
        this IHostApplicationBuilder builder
    )
    {
        var baseUri = $"{Protocol.HttpOrHttps}://{Application.Chatting}/agents";

        var sentimentPlugin = await builder.ConnectRemoteAgent(
            [$"{baseUri}/summarize", $"{baseUri}/sentiment"]
        );

        return sentimentPlugin;
    }

    /// <summary>
    /// Connects to multiple remote agents using their URIs, retrieves their service endpoints, creates agent proxies, and aggregates their kernel functions into a single kernel plugin.
    /// </summary>
    /// <param name="agentUris">An array of URIs identifying the remote agents to connect to.</param>
    /// <returns>A kernel plugin containing the combined functions of the connected remote agents.</returns>
    private static async Task<KernelPlugin> ConnectRemoteAgent(
        this IHostApplicationBuilder builder,
        string[] agentUris
    )
    {
        var serviceEndpointResolver = builder
            .Services.BuildServiceProvider()
            .GetRequiredService<ServiceEndpointResolver>();

        List<string> endpointUrls = [];

        endpointUrls.AddRange(
            await Task.WhenAll(
                agentUris.Select(uri => serviceEndpointResolver.ResolveServiceEndpointUrl(uri, "/"))
            )
        );

        var agents = await Task.WhenAll(endpointUrls.Select(uri => uri.CreateAgentAsync()));
        var agentFunctions = agents.Select(agent =>
            AgentKernelFunctionFactory.CreateFromAgent(agent)
        );

        return KernelPluginFactory.CreateFromFunctions("AgentPlugin", agentFunctions);
    }
}

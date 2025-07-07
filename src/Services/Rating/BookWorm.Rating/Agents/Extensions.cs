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

    public static void MapHostRatingAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(nameof(RatingAgent));
        var hostAgent = new A2AHostAgent(agent, RatingAgent.GetAgentCard());

        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new(1, 0))
            .ReportApiVersions()
            .Build();

        app.MapA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/rating")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(RatingAgent));

        app.MapHttpA2A(hostAgent.TaskManager!, "/api/v{version:apiVersion}/agents/rating")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(new(1, 0))
            .WithTags(nameof(RatingAgent));
    }

    private static async Task<KernelPlugin> GetRemoteAgentPlugin(
        this IHostApplicationBuilder builder
    )
    {
        var baseUri = $"{Protocol.HttpOrHttps}://{Application.Chatting}/api/v1/agents";

        var sentimentPlugin = await builder.ConnectRemoteAgent(
            [$"{baseUri}/summarize", $"{baseUri}/sentiment"]
        );

        return sentimentPlugin;
    }

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

using BookWorm.Chassis.AI.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Extensions;

public static class A2AClientExtensions
{
    public static void AddA2AClient(
        this IServiceCollection services,
        string agentName,
        string agentUri,
        string? path = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentUri);

        services.AddHttpClient<AgentDiscoveryClient>(
            agentName,
            client => client.BaseAddress = new(agentUri)
        );

        services.AddSingleton(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(agentName);

            var uriBuilder = new UriBuilder(httpClient.BaseAddress!);

            if (!string.IsNullOrWhiteSpace(path))
            {
                uriBuilder.Path = path;
            }

            return new A2AAgentClient(uriBuilder.Uri);
        });
    }
}

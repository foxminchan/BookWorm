using A2A;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Agents.A2A;

namespace BookWorm.Chassis.RAG.Extensions;

public static class A2AClientExtensions
{
    public static void AddA2AClient(
        this IServiceCollection services,
        string agentName,
        string agentUri
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);
        ArgumentException.ThrowIfNullOrWhiteSpace(agentUri);

        if (!Uri.TryCreate(agentUri, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
        {
            throw new ArgumentException(
                "Agent URI must be a valid absolute URI.",
                nameof(agentUri)
            );
        }

        services.AddHttpClient(agentName, client => client.BaseAddress = uri);

        services.AddKeyedSingleton(
            agentName,
            (sp, _) =>
            {
                var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(agentName);
                return CreateAgentAsync(httpClient).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        );
    }

    private static async Task<A2AAgent> CreateAgentAsync(HttpClient httpClient)
    {
        var baseUrl = httpClient.BaseAddress;

        ArgumentNullException.ThrowIfNull(baseUrl);

        var client = new A2AClient(baseUrl, httpClient);
        var cardResolver = new A2ACardResolver(baseUrl, httpClient);
        var agentCard = await cardResolver.GetAgentCardAsync();

        return new(client, agentCard);
    }
}

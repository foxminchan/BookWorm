using Microsoft.SemanticKernel.Agents.A2A;
using SharpA2A.Core;

namespace BookWorm.Chassis.RAG.Agent;

public static class A2AExtensions
{
    public static async Task<A2AAgent> CreateAgentAsync(this string agentUri)
    {
        if (!Uri.TryCreate(agentUri, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Invalid URI format", nameof(agentUri));
        }

        var httpClient = new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(60) };

        var client = new A2AClient(httpClient);
        var cardResolver = new A2ACardResolver(httpClient);
        var agentCard = await cardResolver.GetAgentCardAsync();

        return new(client, agentCard);
    }
}

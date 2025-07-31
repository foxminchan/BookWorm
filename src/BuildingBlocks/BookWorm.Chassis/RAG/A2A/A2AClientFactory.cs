using A2A;

namespace BookWorm.Chassis.RAG.A2A;

public static class A2AClientFactory
{
    public static async Task<A2AAgent> CreateAgentAsync(string agentUri, HttpClient httpClient)
    {
        if (!Uri.TryCreate(agentUri, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Invalid URI format", nameof(agentUri));
        }

        var client = new A2AClient(uri, httpClient);
        var cardResolver = new A2ACardResolver(uri, httpClient);

        var agentCard = await cardResolver.GetAgentCardAsync();

        return new(client, agentCard);
    }
}

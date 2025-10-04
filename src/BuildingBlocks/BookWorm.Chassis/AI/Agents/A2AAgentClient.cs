using System.Collections.Concurrent;
using A2A;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

public sealed class A2AAgentClient(Uri baseUri)
{
    private readonly ConcurrentDictionary<string, (A2AClient, A2ACardResolver)> _clients = [];

    public AIAgent GetAIAgent(string agentName)
    {
        var (agentClient, _) = ResolveClient(agentName);

        var agent = agentClient.GetAIAgent();

        return agent;
    }

    public async Task<AgentCard?> GetAgentCardAsync(
        string agentName,
        CancellationToken cancellationToken = default
    )
    {
        var (_, resolver) = ResolveClient(agentName);

        try
        {
            return await resolver.GetAgentCardAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private (A2AClient, A2ACardResolver) ResolveClient(string agentName)
    {
        return _clients.GetOrAdd(
            agentName,
            name =>
            {
                var uri = new Uri($"{baseUri}/{name}/");

                var client = new A2AClient(uri);

                var resolver = new A2ACardResolver(uri, agentCardPath: "/v1/card/");

                return (client, resolver);
            }
        );
    }
}

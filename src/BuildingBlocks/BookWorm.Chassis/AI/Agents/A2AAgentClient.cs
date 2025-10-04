using A2A;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

public sealed class A2AAgentClient(Uri baseUri, string? path)
{
    public async Task<AIAgent> GetAIAgent(string agentName)
    {
        var agent = await ResolveClient(agentName).GetAIAgentAsync();

        return agent;
    }

    public async Task<AgentCard?> GetAgentCardAsync(
        string agentName,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await ResolveClient(agentName).GetAgentCardAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private A2ACardResolver ResolveClient(string agentName)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new($"{baseUri}"),
            Timeout = TimeSpan.FromMinutes(2),
        };

        var resolver = new A2ACardResolver(
            httpClient.BaseAddress,
            httpClient,
            $"/{path?.TrimStart('/')}/{agentName}/v1/card/"
        );

        return resolver;
    }
}

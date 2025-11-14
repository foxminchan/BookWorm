using System.Security.Claims;
using BookWorm.Chassis.Security.TokenExchange;
using BookWorm.Chassis.Utilities;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

public static class A2AClientFactory
{
    public static AIAgent CreateA2AAgentClient(
        string serviceName,
        string agentName,
        string? path = "a2a",
        ITokenExchange? tokenExchange = null,
        ClaimsPrincipal? claimsPrincipal = null
    )
    {
        var baseAddress =
            ServiceDiscoveryUtilities.GetServiceEndpoint(serviceName)
            ?? throw new InvalidOperationException(
                $"Service endpoint for agent '{serviceName}' not found."
            );

        var agent = new A2AAgentClient(new(baseAddress), path).GetAIAgent(
            agentName,
            tokenExchange,
            claimsPrincipal
        );

        return agent.GetAwaiter().GetResult();
    }
}

using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.Utilities;
using BookWorm.Constants.Aspire;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Extensions;

public static class A2AClientExtensions
{
    public static AIAgent GetA2AAgent(string serviceName, string agentName, string? path = "a2a")
    {
        var baseAddress =
            ServiceDiscoveryUtilities.GetServiceEndpoint(serviceName, Protocols.Https)
            ?? ServiceDiscoveryUtilities.GetServiceEndpoint(serviceName, Protocols.Http)
            ?? throw new InvalidOperationException(
                $"Service endpoint for agent '{serviceName}' not found."
            );

        var agent = new A2AAgentClient(new(baseAddress), path).GetAIAgent(agentName);

        return agent.GetAwaiter().GetResult();
    }
}

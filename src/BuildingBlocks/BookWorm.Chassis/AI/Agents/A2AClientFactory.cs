using BookWorm.Chassis.Utilities;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

public static class A2AClientFactory
{
    /// <summary>
    ///     Creates an A2A agent client for communicating with a remote agent.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="serviceName">The name of the service hosting the agent.</param>
    /// <param name="agentName">The name of the agent to connect to.</param>
    /// <param name="agentClientId">
    ///     The client ID for authentication. Required if the agent has authentication enabled;
    ///     otherwise, <c>null</c>.
    /// </param>
    /// <param name="path">The path to the A2A endpoint. Defaults to <c>"a2a"</c>.</param>
    /// <returns>An <see cref="AIAgent" /> instance configured to communicate with the specified agent.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service endpoint cannot be found.</exception>
    /// <remarks>
    ///     This method performs service discovery to locate the agent endpoint and establishes an A2A connection.
    ///     The <paramref name="agentClientId" /> parameter must be provided if the target agent requires authentication.
    /// </remarks>
    public static AIAgent CreateA2AAgentClient(
        IServiceProvider serviceProvider,
        string serviceName,
        string agentName,
        string? agentClientId = null,
        string? path = "a2a"
    )
    {
        var baseAddress = ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(serviceName);

        var agentClient = new A2AAgentClient(new(baseAddress), path);

        var agent = agentClient.GetAIAgent(serviceProvider, agentName, agentClientId);

        return agent.GetAwaiter().GetResult();
    }
}

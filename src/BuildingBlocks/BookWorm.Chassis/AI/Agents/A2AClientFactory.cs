using BookWorm.Chassis.Utilities;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

public static class A2AClientFactory
{
    /// <summary>
    ///     Creates an A2A agent client that communicates with a remote agent over gRPC.
    /// </summary>
    /// <param name="serviceName">The name of the service hosting the agent.</param>
    /// <param name="agentName">The name of the agent to connect to.</param>
    /// <param name="httpClient">
    ///     The HTTP client used for A2A requests. Configure authentication handlers on this client
    ///     so credentials are resolved for each request.
    /// </param>
    /// <returns>An <see cref="AIAgent" /> instance configured to communicate with the specified agent.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service endpoint cannot be found.</exception>
    public static AIAgent CreateA2AAgentClient(
        string serviceName,
        string agentName,
        HttpClient httpClient
    )
    {
        var baseAddress = ServiceDiscoveryUtilities.GetRequiredServiceEndpoint(serviceName);

        return new A2AAgentClient(new(baseAddress)).GetAIAgent(agentName, httpClient);
    }
}

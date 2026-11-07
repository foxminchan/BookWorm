using A2A;
using A2A.Grpc;
using Microsoft.Agents.AI;

namespace BookWorm.Chassis.AI.Agents;

internal sealed class A2AAgentClient(Uri baseUri)
{
    static A2AAgentClient()
    {
        A2AGrpcClientRegistration.Register();
    }

    /// <summary>
    ///     Creates an <see cref="AIAgent" /> that calls the remote agent over the A2A gRPC binding.
    /// </summary>
    /// <param name="agentName">The name of the remote agent.</param>
    /// <param name="httpClient">
    ///     The HTTP client used for the gRPC channel. Authentication must be applied per request
    ///     (for example with a delegating handler), because the returned agent is long-lived.
    /// </param>
    public AIAgent GetAIAgent(string agentName, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        AgentCard card = new()
        {
            Name = agentName,
            SupportedInterfaces =
            [
                new() { Url = baseUri.ToString(), ProtocolBinding = ProtocolBindingNames.Grpc },
            ],
        };

        return card.AsAIAgent(
            httpClient,
            new() { PreferredBindings = [ProtocolBindingNames.Grpc] }
        );
    }
}

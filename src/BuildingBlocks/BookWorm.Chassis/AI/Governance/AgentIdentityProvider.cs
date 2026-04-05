using System.Collections.Concurrent;
using AgentGovernance;
using AgentGovernance.Trust;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance;

public sealed class AgentIdentityProvider(
    GovernanceKernel kernel,
    ILogger<AgentIdentityProvider> logger
)
{
    private readonly ConcurrentDictionary<string, AgentIdentity> _identities = new();

    /// <summary>
    ///     Gets or creates a DID-based identity for the specified agent.
    /// </summary>
    /// <param name="agentName">The logical agent name (e.g., "BookAgent").</param>
    /// <returns>The agent's cryptographic identity.</returns>
    public AgentIdentity GetOrCreateIdentity(string agentName)
    {
        return _identities.GetOrAdd(
            agentName,
            name =>
            {
                var identity = AgentIdentity.Create(name);
                logger.LogInformation(
                    "Created governance identity for agent {AgentName}: {Did}",
                    name,
                    identity.Did
                );
                return identity;
            }
        );
    }

    /// <summary>
    ///     Records a positive trust signal for the specified agent (successful tool call).
    /// </summary>
    public void RecordSuccess(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(
            allowed: true,
            identity.Did,
            "success_signal",
            evaluationMs: 0
        );
    }

    /// <summary>
    ///     Records a negative trust signal for the specified agent (blocked or failed tool call).
    /// </summary>
    public void RecordFailure(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(
            allowed: false,
            identity.Did,
            "failure_signal",
            evaluationMs: 0
        );
    }
}

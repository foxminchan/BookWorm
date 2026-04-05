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
    ///     Gets an existing agent identity or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to get or create an identity.</param>
    /// <returns>An <see cref="AgentIdentity" /> object representing the agent's governance identity.</returns>
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
    ///     Records a successful governance decision signal for the specified agent.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to record the success signal.</param>
    public void RecordSuccess(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(true, identity.Did, "success_signal", 0);
    }

    /// <summary>
    ///     Records a failed governance decision signal for the specified agent.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to record the failure signal.</param>
    public void RecordFailure(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(false, identity.Did, "failure_signal", 0);
    }
}

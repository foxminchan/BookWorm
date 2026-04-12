using System.Collections.Concurrent;
using AgentGovernance;
using AgentGovernance.Trust;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance;

internal sealed class AgentIdentityProvider(
    GovernanceKernel kernel,
    ILogger<AgentIdentityProvider> logger
) : IAgentIdentityProvider
{
    private readonly ConcurrentDictionary<string, AgentIdentity> _identities = new();

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

    public void RecordSuccess(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(true, identity.Did, "success_signal", 0);
    }

    public void RecordFailure(string agentName)
    {
        var identity = GetOrCreateIdentity(agentName);
        kernel.Metrics?.RecordDecision(false, identity.Did, "failure_signal", 0);
    }
}

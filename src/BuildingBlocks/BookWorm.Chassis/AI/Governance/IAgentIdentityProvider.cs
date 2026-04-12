using AgentGovernance.Trust;

namespace BookWorm.Chassis.AI.Governance;

public interface IAgentIdentityProvider
{
    /// <summary>
    ///     Gets an existing agent identity or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to get or create an identity.</param>
    /// <returns>An <see cref="AgentIdentity" /> object representing the agent's governance identity.</returns>
    AgentIdentity GetOrCreateIdentity(string agentName);

    /// <summary>
    ///     Records a successful governance decision signal for the specified agent.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to record the success signal.</param>
    void RecordSuccess(string agentName);

    /// <summary>
    ///     Records a failed governance decision signal for the specified agent.
    /// </summary>
    /// <param name="agentName">The name of the agent for which to record the failure signal.</param>
    void RecordFailure(string agentName);
}

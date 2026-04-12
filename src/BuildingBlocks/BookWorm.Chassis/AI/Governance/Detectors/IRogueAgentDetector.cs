namespace BookWorm.Chassis.AI.Governance.Detectors;

public interface IRogueAgentDetector
{
    /// <summary>
    ///     Records a tool call for the specified agent and returns the current anomaly score.
    /// </summary>
    /// <param name="agentId">The agent identifier (DID or logical name).</param>
    /// <param name="toolName">The name of the tool being invoked.</param>
    /// <param name="windowSize">Number of recent calls to consider for Z-score calculation.</param>
    /// <param name="zThreshold">Z-score threshold above which behavior is flagged as anomalous.</param>
    /// <returns>An <see cref="AnomalyScore" /> reflecting the current behavioral analysis.</returns>
    AnomalyScore RecordCall(
        string agentId,
        string toolName,
        int windowSize = 20,
        double zThreshold = 2.5
    );

    /// <summary>
    ///     Checks whether the specified agent is currently quarantined.
    /// </summary>
    /// <param name="agentId">The agent identifier to check.</param>
    /// <returns><c>true</c> if the agent is quarantined; otherwise <c>false</c>.</returns>
    bool IsQuarantined(string agentId);

    /// <summary>
    ///     Releases an agent from quarantine, allowing it to resume operations.
    ///     Typically called after human review.
    /// </summary>
    /// <param name="agentId">The agent identifier to release.</param>
    /// <returns><c>true</c> if the agent was quarantined and is now released; otherwise <c>false</c>.</returns>
    bool ReleaseFromQuarantine(string agentId);
}

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance;

public sealed record AnomalyScore(
    double ZScore,
    double Entropy,
    double CapabilityDeviation,
    bool IsAnomalous,
    bool Quarantine
);

public sealed class RogueAgentDetector(ILogger<RogueAgentDetector> logger)
{
    private readonly ConcurrentDictionary<string, AgentCallProfile> _profiles = new();

    public IReadOnlySet<string> QuarantinedAgents =>
        _profiles.Where(kvp => kvp.Value.IsQuarantined).Select(kvp => kvp.Key).ToHashSet();

    /// <summary>
    ///     Records a tool call for the specified agent and returns the current anomaly score.
    /// </summary>
    /// <param name="agentId">The agent identifier (DID or logical name).</param>
    /// <param name="toolName">The name of the tool being invoked.</param>
    /// <param name="windowSize">Number of recent calls to consider for Z-score calculation.</param>
    /// <param name="zThreshold">Z-score threshold above which behavior is flagged as anomalous.</param>
    /// <returns>An <see cref="AnomalyScore" /> reflecting the current behavioral analysis.</returns>
    public AnomalyScore RecordCall(
        string agentId,
        string toolName,
        int windowSize = 20,
        double zThreshold = 2.5
    )
    {
        var profile = _profiles.GetOrAdd(agentId, _ => new());
        var score = profile.RecordAndAnalyze(toolName, windowSize, zThreshold);

        if (score.Quarantine && !profile.IsQuarantined)
        {
            profile.IsQuarantined = true;
            logger.LogCritical(
                "Rogue agent detected — quarantining {AgentId}: ZScore={ZScore:F2}, Entropy={Entropy:F3}, CapDev={CapDev:F3}",
                agentId,
                score.ZScore,
                score.Entropy,
                score.CapabilityDeviation
            );
        }
        else if (score.IsAnomalous)
        {
            logger.LogWarning(
                "Anomalous behavior detected for {AgentId}: ZScore={ZScore:F2}, Entropy={Entropy:F3}, CapDev={CapDev:F3}",
                agentId,
                score.ZScore,
                score.Entropy,
                score.CapabilityDeviation
            );
        }

        return score;
    }

    /// <summary>
    ///     Checks whether the specified agent is currently quarantined.
    /// </summary>
    /// <param name="agentId">The agent identifier to check.</param>
    /// <returns><c>true</c> if the agent is quarantined; otherwise <c>false</c>.</returns>
    public bool IsQuarantined(string agentId)
    {
        return _profiles.TryGetValue(agentId, out var profile) && profile.IsQuarantined;
    }

    /// <summary>
    ///     Releases an agent from quarantine, allowing it to resume operations.
    ///     Typically called after human review.
    /// </summary>
    /// <param name="agentId">The agent identifier to release.</param>
    /// <returns><c>true</c> if the agent was quarantined and is now released; otherwise <c>false</c>.</returns>
    public bool ReleaseFromQuarantine(string agentId)
    {
        if (!_profiles.TryGetValue(agentId, out var profile) || !profile.IsQuarantined)
        {
            return false;
        }

        profile.IsQuarantined = false;
        logger.LogInformation(
            "Agent {AgentId} released from quarantine after human review",
            agentId
        );
        return true;
    }

    private sealed class AgentCallProfile
    {
        private const int MinSamplesForAnalysis = 5;
        private const double MinStdDev = 0.001;

        private readonly List<double> _callTimestamps = [];
        private readonly Lock _lock = new();
        private readonly Dictionary<string, int> _toolCounts = [];

        public volatile bool IsQuarantined;

        public AnomalyScore RecordAndAnalyze(string toolName, int windowSize, double zThreshold)
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
                _callTimestamps.Add(now);
                _toolCounts[toolName] = _toolCounts.GetValueOrDefault(toolName) + 1;

                if (_callTimestamps.Count < MinSamplesForAnalysis)
                {
                    return new(0, 0, 0, false, false);
                }

                var zScore = ComputeZScore(windowSize);
                var entropy = ComputeEntropy();
                var capDev = ComputeCapabilityDeviation();

                var anomalous = zScore > zThreshold || capDev > 0.8;
                var quarantine = zScore > zThreshold * 1.5 || (anomalous && capDev > 0.85);

                return new(
                    Math.Round(zScore, 2),
                    Math.Round(entropy, 3),
                    Math.Round(capDev, 3),
                    anomalous,
                    quarantine
                );
            }
        }

        private double ComputeZScore(int windowSize)
        {
            var recent = _callTimestamps.TakeLast(windowSize).ToList();

            if (recent.Count < 2)
            {
                return 0;
            }

            var intervals = new List<double>(recent.Count - 1);
            for (var i = 1; i < recent.Count; i++)
            {
                intervals.Add(recent[i] - recent[i - 1]);
            }

            var mean = intervals.Average();
            var std = Math.Sqrt(intervals.Average(x => Math.Pow(x - mean, 2)));

            if (std < MinStdDev)
            {
                std = MinStdDev;
            }

            return Math.Abs((intervals[^1] - mean) / std);
        }

        private double ComputeEntropy()
        {
            var total = _toolCounts.Values.Sum();

            return _toolCounts
                .Values.Select(count => (double)count / total)
                .Where(p => p > 0)
                .Aggregate<double, double>(0, (current, p) => current - (p * Math.Log2(p)));
        }

        private double ComputeCapabilityDeviation()
        {
            var total = _toolCounts.Values.Sum();
            var maxCount = _toolCounts.Values.Max();
            return (double)maxCount / total;
        }
    }
}

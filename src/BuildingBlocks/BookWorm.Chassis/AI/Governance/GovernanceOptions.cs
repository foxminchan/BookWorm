using System.ComponentModel.DataAnnotations;

namespace BookWorm.Chassis.AI.Governance;

/// <summary>
///     Configuration options for the Agent Governance Toolkit integration.
/// </summary>
public sealed class AgentGovernanceOptions
{
    /// <summary>
    ///     The configuration section name in appsettings.json.
    /// </summary>
    public const string ConfigurationSection = "AgentGovernance";

    /// <summary>
    ///     Paths to YAML policy files, relative to the application base directory.
    /// </summary>
    [Required]
    public List<string> PolicyPaths { get; set; } = ["AI/Governance/Policies/default.yaml"];

    /// <summary>
    ///     Whether to enable execution ring enforcement based on agent trust scores.
    /// </summary>
    public bool EnableRings { get; set; } = true;

    /// <summary>
    ///     Whether to enable the built-in prompt injection detector.
    /// </summary>
    public bool EnablePromptInjectionDetection { get; set; } = true;

    /// <summary>
    ///     Whether to enable the circuit breaker for governance evaluations.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    ///     Default trust score for newly registered agents (0–1000).
    /// </summary>
    public int DefaultTrustScore { get; set; } = 500;

    /// <summary>
    ///     Trust score decay rate per hour without positive signals.
    /// </summary>
    public int TrustDecayRate { get; set; } = 10;
}

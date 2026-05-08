using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.AI.Governance;

[OptionsValidator]
public sealed partial class AgentGovernanceOptions : IValidateOptions<AgentGovernanceOptions>
{
    public const string ConfigurationSection = "AgentGovernance";

    [Required]
    [MinLength(1)]
    public List<string> PolicyPaths { get; set; } = ["AI/Governance/Policies/default.yaml"];

    public bool EnableRings { get; set; } = true;

    public bool EnablePromptInjectionDetection { get; set; } = true;

    public bool EnableCircuitBreaker { get; set; } = true;
}

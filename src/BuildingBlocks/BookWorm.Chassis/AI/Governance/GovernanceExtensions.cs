using AgentGovernance;
using AgentGovernance.Audit;
using AgentGovernance.Policy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance;

/// <summary>
///     Extension methods for registering Agent Governance Toolkit services.
/// </summary>
public static class GovernanceExtensions
{
    /// <summary>
    ///     Adds the Agent Governance Toolkit with policy enforcement, prompt injection detection,
    ///     execution rings, circuit breaker, and OpenTelemetry metrics.
    /// </summary>
    public static IHostApplicationBuilder AddAgentGovernance(
        this IHostApplicationBuilder builder,
        params string[] additionalPolicyPaths
    )
    {
        var services = builder.Services;

        // Bind configuration
        var section = builder.Configuration.GetSection(AgentGovernanceOptions.ConfigurationSection);
        services.Configure<AgentGovernanceOptions>(section);

        var options = new AgentGovernanceOptions();
        section.Bind(options);

        // Resolve absolute policy paths from the base directory
        var policyPaths = options
            .PolicyPaths.Concat(additionalPolicyPaths)
            .Select(p => Path.Combine(AppContext.BaseDirectory, p))
            .Where(File.Exists)
            .Distinct()
            .ToList();

        // Create and register the GovernanceKernel as a singleton
        var governanceOptions = new GovernanceOptions
        {
            PolicyPaths = policyPaths,
            ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
            EnableRings = options.EnableRings,
            EnablePromptInjectionDetection = options.EnablePromptInjectionDetection,
            EnableCircuitBreaker = options.EnableCircuitBreaker,
        };

        var kernel = new GovernanceKernel(governanceOptions);

        services.AddSingleton(kernel);
        services.AddSingleton<AgentIdentityProvider>();

        // Wire audit events to logging
        services.AddSingleton<IHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<GovernanceKernel>>();
            return new GovernanceAuditHostedService(kernel, logger);
        });

        // Register OpenTelemetry meters for governance
        services.AddOpenTelemetry().WithMetrics(m => m.AddMeter("agent_governance"));

        return builder;
    }
}

/// <summary>
///     Hosted service that wires governance audit events to the logging infrastructure.
/// </summary>
internal sealed class GovernanceAuditHostedService(
    GovernanceKernel kernel,
    ILogger<GovernanceKernel> logger
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        kernel.OnEvent(
            GovernanceEventType.ToolCallBlocked,
            evt =>
                logger.LogWarning(
                    "Governance blocked tool call: Agent={AgentId}, Tool={Tool}, Reason={Reason}",
                    evt.AgentId,
                    evt.Data.GetValueOrDefault("tool_name", "unknown"),
                    evt.Data.GetValueOrDefault("reason", "policy denied")
                )
        );

        kernel.OnAllEvents(evt =>
            logger.LogDebug("Governance event: Agent={AgentId}", evt.AgentId)
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

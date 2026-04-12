using AgentGovernance;
using AgentGovernance.Policy;
using BookWorm.Chassis.AI.Governance.AuditTrail;
using BookWorm.Chassis.AI.Governance.Detectors;
using BookWorm.Chassis.AI.Governance.IdentityProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.AI.Governance;

public static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds agent governance capabilities to the host application builder, enabling policy enforcement,
        ///     conflict resolution, rogue agent detection, Merkle-chained audit logging, and
        ///     trust-based execution ring enforcement for AI agent operations.
        /// </summary>
        /// <param name="paths">Additional policy file paths to be loaded alongside configured paths.</param>
        /// <returns>The configured host application builder for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if builder is null.</exception>
        public IHostApplicationBuilder AddAgentGovernance(params string[] paths)
        {
            var services = builder.Services;

            // Bind configuration
            var section = builder.Configuration.GetSection(
                AgentGovernanceOptions.ConfigurationSection
            );

            services
                .AddOptionsWithValidateOnStart<AgentGovernanceOptions>()
                .Bind(section)
                .ValidateDataAnnotations();

            // Create and register the GovernanceKernel as a singleton
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AgentGovernanceOptions>>().Value;

                // Resolve absolute policy paths from the base directory
                var policyPaths = options
                    .PolicyPaths.Concat(paths)
                    .Select(p => Path.Combine(AppContext.BaseDirectory, p))
                    .Where(File.Exists)
                    .Distinct()
                    .ToList();

                var governanceOptions = new GovernanceOptions
                {
                    PolicyPaths = policyPaths,
                    ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
                    EnableRings = options.EnableRings,
                    EnablePromptInjectionDetection = options.EnablePromptInjectionDetection,
                    EnableCircuitBreaker = options.EnableCircuitBreaker,
                };

                return new GovernanceKernel(governanceOptions);
            });

            // Identity provider for agents to track trust scores and audit entries
            services.AddSingleton<IAgentIdentityProvider, AgentIdentityProvider>();

            // Rogue agent detection — Z-score frequency analysis with auto-quarantine
            services.AddSingleton<IRogueAgentDetector, RogueAgentDetector>();

            // Merkle-chained audit trail — tamper-proof compliance logging
            services.AddSingleton<IGovernanceAuditTrail, GovernanceAuditTrail>();

            // Wire audit events to logging and Merkle chain
            services.AddSingleton<IHostedService, GovernanceAuditHostedService>();

            // Register OpenTelemetry meters for governance
            services.AddOpenTelemetry().WithMetrics(m => m.AddMeter("agent_governance"));

            return builder;
        }
    }
}

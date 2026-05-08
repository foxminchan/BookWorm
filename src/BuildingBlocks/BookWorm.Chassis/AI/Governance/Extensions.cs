using AgentGovernance;
using AgentGovernance.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookWorm.Chassis.AI.Governance;

public static class Extensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers the shared <see cref="GovernanceKernel" /> used by the official
        ///     <c>Microsoft.AgentGovernance.Extensions.Microsoft.Agents</c> and
        ///     <c>Microsoft.AgentGovernance.Extensions.ModelContextProtocol</c> integrations.
        ///     Bind <see cref="AgentGovernanceOptions" /> from configuration and merge any
        ///     additional policy paths supplied at the call site.
        /// </summary>
        /// <param name="paths">Additional policy file paths (relative to the application base directory).</param>
        /// <returns>The configured host application builder for method chaining.</returns>
        public IHostApplicationBuilder AddAgentGovernance(params string[] paths)
        {
            var services = builder.Services;

            services
                .AddOptionsWithValidateOnStart<AgentGovernanceOptions>()
                .Bind(builder.Configuration.GetSection(AgentGovernanceOptions.ConfigurationSection))
                .ValidateDataAnnotations();

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AgentGovernanceOptions>>().Value;

                var policyPaths = options
                    .PolicyPaths.Concat(paths)
                    .Select(p => Path.Combine(AppContext.BaseDirectory, p))
                    .Where(File.Exists)
                    .Distinct()
                    .ToList();

                return new GovernanceKernel(
                    new()
                    {
                        PolicyPaths = policyPaths,
                        ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
                        EnableRings = options.EnableRings,
                        EnablePromptInjectionDetection = options.EnablePromptInjectionDetection,
                        EnableCircuitBreaker = options.EnableCircuitBreaker,
                    }
                );
            });

            services.AddOpenTelemetry().WithMetrics(m => m.AddMeter("agent_governance"));

            return builder;
        }
    }
}

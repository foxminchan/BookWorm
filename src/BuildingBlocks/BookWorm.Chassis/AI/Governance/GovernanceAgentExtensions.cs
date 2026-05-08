using AgentGovernance;
using AgentGovernance.Extensions.Microsoft.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.AI.Governance;

public static class GovernanceAgentExtensions
{
    extension(AIAgent agent)
    {
        /// <summary>
        ///     Wraps an <see cref="AIAgent" /> with the official
        ///     <c>Microsoft.AgentGovernance.Extensions.Microsoft.Agents</c> hook, resolving the shared
        ///     <see cref="GovernanceKernel" /> from DI. Function-call middleware is enabled so
        ///     governance policies are enforced for every tool invocation.
        /// </summary>
        /// <param name="services">The DI service provider that exposes the registered <see cref="GovernanceKernel" />.</param>
        /// <param name="agentName">Logical agent name; used to build the default DID (<c>did:bookworm:{agentName}</c>).</param>
        public AIAgent WithBookWormGovernance(IServiceProvider services, string agentName)
        {
            return agent.WithGovernance(
                services.GetRequiredService<GovernanceKernel>(),
                new()
                {
                    DefaultAgentId = $"did:bookworm:{agentName}",
                    EnableFunctionMiddleware = true,
                }
            );
        }
    }
}

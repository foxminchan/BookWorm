using AgentGovernance;
using AgentGovernance.Audit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance;

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

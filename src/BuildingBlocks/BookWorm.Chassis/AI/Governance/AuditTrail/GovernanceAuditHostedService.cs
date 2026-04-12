using AgentGovernance;
using AgentGovernance.Audit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Governance.AuditTrail;

internal sealed class GovernanceAuditHostedService(
    GovernanceKernel kernel,
    ILogger<GovernanceKernel> logger,
    IGovernanceAuditTrail auditTrail
) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        kernel.OnEvent(
            GovernanceEventType.ToolCallBlocked,
            evt =>
            {
                var toolName = evt.Data.GetValueOrDefault("tool_name", "unknown");
                var reason = evt.Data.GetValueOrDefault("reason", "policy denied");

                logger.LogWarning(
                    "Governance blocked tool call: Agent={AgentId}, Tool={Tool}, Reason={Reason}",
                    evt.AgentId,
                    toolName,
                    reason
                );

                auditTrail.Log(evt.AgentId, "tool_blocked", "deny", $"{toolName}: {reason}");
            }
        );

        kernel.OnAllEvents(evt =>
        {
            logger.LogDebug("Governance event: Agent={AgentId}", evt.AgentId);
            auditTrail.Log(evt.AgentId, evt.Type.ToString(), "audit", evt.AgentId);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        var (isValid, count) = auditTrail.VerifyIntegrity();

        if (isValid)
        {
            logger.LogInformation(
                "Governance audit trail integrity verified: {Count} entries, chain intact",
                count
            );
        }
        else
        {
            logger.LogCritical(
                "Governance audit trail INTEGRITY FAILURE at entry {Index} — possible tampering detected",
                count
            );
        }

        return Task.CompletedTask;
    }
}

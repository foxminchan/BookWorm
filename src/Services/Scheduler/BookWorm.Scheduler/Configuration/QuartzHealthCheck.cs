using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz.Impl.Matchers;

namespace BookWorm.Scheduler.Configuration;

/// <summary>
/// Health check that verifies the Quartz scheduler is running and all triggers are in a healthy state.
/// </summary>
internal sealed class QuartzHealthCheck(ISchedulerFactory schedulerFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        if (!scheduler.IsStarted || scheduler.IsShutdown)
        {
            return HealthCheckResult.Unhealthy("Quartz scheduler is not running.");
        }

        if (scheduler.InStandbyMode)
        {
            return HealthCheckResult.Degraded("Quartz scheduler is in standby mode.");
        }

        var triggerKeys = await scheduler.GetTriggerKeys(
            GroupMatcher<TriggerKey>.AnyGroup(),
            cancellationToken
        );

        foreach (var triggerKey in triggerKeys)
        {
            var state = await scheduler.GetTriggerState(triggerKey, cancellationToken);

            if (state is TriggerState.Error)
            {
                return HealthCheckResult.Unhealthy(
                    $"Trigger '{triggerKey.Name}' in group '{triggerKey.Group}' is in error state."
                );
            }
        }

        return HealthCheckResult.Healthy(
            $"Quartz scheduler is running with {triggerKeys.Count} trigger(s)."
        );
    }
}

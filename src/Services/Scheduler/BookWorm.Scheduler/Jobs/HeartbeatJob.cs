namespace BookWorm.Scheduler.Jobs;

internal sealed class HeartbeatJob(ILogger<HeartbeatJob> logger) : IJob
{
    public ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "Heartbeat fired at {FireTime}, next at {NextFireTime}",
            context.FireTimeUtc,
            context.NextFireTimeUtc
        );

        return ValueTask.CompletedTask;
    }
}

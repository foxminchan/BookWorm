using BookWorm.Contracts;

namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class CleanUpSentEmailJob(IBus bus, ILogger<CleanUpSentEmailJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await bus.Publish(new CleanUpSentEmailIntegrationEvent(), context.CancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventName}",
                nameof(CleanUpSentEmailIntegrationEvent)
            );
            throw new JobExecutionException(ex, false);
        }
    }
}

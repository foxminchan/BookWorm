using BookWorm.Contracts;

namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
public sealed class ResendErrorEmailJob(IBus bus, ILogger<ResendErrorEmailJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await bus.Publish(new ResendErrorEmailIntegrationEvent(), context.CancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventName}",
                nameof(ResendErrorEmailIntegrationEvent)
            );
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}

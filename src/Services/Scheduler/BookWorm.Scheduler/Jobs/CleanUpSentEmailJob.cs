using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class CleanUpSentEmailJob(IMessageBus bus, ILogger<CleanUpSentEmailJob> logger)
    : IJob
{
    public async ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await bus.PublishAsync(new CleanUpSentEmailIntegrationEvent());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventName}",
                nameof(CleanUpSentEmailIntegrationEvent)
            );
            throw new JobExecutionException(ex);
        }
    }
}

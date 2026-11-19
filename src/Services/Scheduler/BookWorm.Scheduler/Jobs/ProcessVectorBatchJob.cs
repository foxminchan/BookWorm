namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class ProcessVectorBatchJob(IMessageBus bus, ILogger<ProcessVectorBatchJob> logger)
    : IJob
{
    public async ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await bus.PublishAsync(new ProcessVectorBatchIntegrationEvent());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventName}",
                nameof(ProcessVectorBatchIntegrationEvent)
            );
            throw new JobExecutionException(ex);
        }
    }
}

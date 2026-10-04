using BookWorm.Scheduler.Clients;

namespace BookWorm.Scheduler.Jobs;

[DisallowConcurrentExecution]
internal sealed class ProcessVectorBatchJob(
    ICatalogIngestionApi catalogIngestionApi,
    ILogger<ProcessVectorBatchJob> logger
) : IJob
{
    public async ValueTask Execute(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await catalogIngestionApi.ProcessVectorBatchAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to process a vector batch");
            throw new JobExecutionException(ex);
        }
    }
}

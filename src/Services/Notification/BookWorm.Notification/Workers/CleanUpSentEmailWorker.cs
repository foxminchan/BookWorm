using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Notification.Workers;

[DisallowConcurrentExecution]
public sealed class CleanUpSentEmailWorker(
    ILogger<CleanUpSentEmailWorker> logger,
    GlobalLogBuffer logBuffer,
    IServiceScopeFactory scopeFactory
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogDebug("Starting cleanup of sent emails...");
            using var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
            await tableService.BulkDeleteAsync(TablePartition.Processed, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in job execution");
            logBuffer.Flush();
            throw; // Re-throw to let Quartz handle the job failure
        }
    }
}

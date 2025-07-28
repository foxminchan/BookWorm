using BookWorm.Notification.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
            var dbContext = scope.ServiceProvider.GetRequiredService<INotificationDbContext>();

            var sentEmails = await dbContext
                .Outboxes.Where(e => e.IsSent)
                .OrderBy(e => e.SequenceNumber)
                .ToListAsync(context.CancellationToken);

            if (sentEmails.Count != 0)
            {
                logger.LogDebug("Found {Count} sent emails to delete", sentEmails.Count);
                dbContext.Outboxes.RemoveRange(sentEmails);
                await dbContext.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Successfully cleaned up sent emails");
            }
            else
            {
                logger.LogDebug("No sent emails found for cleanup");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in job execution");
            logBuffer.Flush();
            throw; // Re-throw to let Quartz handle the job failure
        }
    }
}

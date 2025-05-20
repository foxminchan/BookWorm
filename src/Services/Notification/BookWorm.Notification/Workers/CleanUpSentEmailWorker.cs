using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

[DisallowConcurrentExecution]
public sealed class CleanUpSentEmailWorker(
    ILogger<CleanUpSentEmailWorker> logger,
    IServiceScopeFactory scopeFactory
) : IJob
{
    private readonly string _partitionKey = nameof(Outbox).ToLowerInvariant();

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogDebug("Starting cleanup of sent emails...");

            using var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

            // Get all sent emails
            var sentEmails = await tableService.ListAsync<Outbox>(_partitionKey);
            var emailsToDelete = sentEmails.Where(e => e.IsSent).ToList();

            if (emailsToDelete.Count == 0)
            {
                logger.LogInformation("No sent emails found to delete.");
                return;
            }

            logger.LogDebug("Found {Count} sent emails to delete.", emailsToDelete.Count);

            // Process in batches to avoid memory issues
            const int batchSize = 100;
            var batches = emailsToDelete.Chunk(batchSize);

            foreach (var batch in batches)
            {
                var tasks = batch.Select(email =>
                    tableService.DeleteAsync(_partitionKey, email.Id.ToString())
                );

                await Task.WhenAll(tasks);
                logger.LogDebug("Successfully deleted batch of {Count} emails", batch.Length);
            }

            logger.LogDebug("Completed cleanup of {Count} sent emails.", emailsToDelete.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in job execution");
            throw; // Re-throw to let Quartz handle the job failure
        }
    }
}

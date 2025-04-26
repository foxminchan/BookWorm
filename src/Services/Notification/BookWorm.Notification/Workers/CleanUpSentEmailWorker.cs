using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

public sealed class CleanUpSentEmailWorker(
    ILogger<CleanUpSentEmailWorker> logger,
    IServiceScopeFactory scopeFactory
) : IHostedService, IDisposable
{
    private readonly string _partitionKey = nameof(Outbox).ToLower();
    private Timer? _timer;

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Clean Up Sent Email Worker is starting.");

        // Calculate time until next midnight
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var timeUntilMidnight = nextMidnight - now;

        _timer = new(
            _ =>
                Task.Run(
                    async () =>
                    {
                        try
                        {
                            await CleanUpSentEmails();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error occurred in timer callback");
                        }
                    },
                    cancellationToken
                ),
            null,
            timeUntilMidnight,
            TimeSpan.FromDays(1) // Run every 24 hours
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Clean Up Sent Email Worker is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async Task CleanUpSentEmails()
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
}

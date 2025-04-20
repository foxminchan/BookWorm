using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

public sealed class CleanUpSentEmailWorker(
    ILogger<CleanUpSentEmailWorker> logger,
    IServiceScopeFactory scopeFactory
) : IHostedService, IDisposable
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Clean Up Sent Email Worker is starting.");

        // Calculate time until next midnight
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var timeUntilMidnight = nextMidnight - now;

        _timer = new(
            async void (_) =>
            {
                try
                {
                    await CleanUpSentEmails();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error occurred while cleaning up sent emails");
                }
            },
            null,
            timeUntilMidnight,
            TimeSpan.FromDays(1) // Run every 24 hours
        );

        return Task.CompletedTask;
    }

    private async Task CleanUpSentEmails()
    {
        try
        {
            logger.LogInformation("Starting cleanup of sent emails...");

            using var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

            // Get all sent emails
            var sentEmails = await tableService.ListAsync<Outbox>("outbox");
            var emailsToDelete = sentEmails.Where(e => e.IsSent).ToList();

            if (emailsToDelete.Count == 0)
            {
                logger.LogInformation("No sent emails found to delete.");
                return;
            }

            logger.LogInformation("Found {Count} sent emails to delete.", emailsToDelete.Count);

            foreach (var email in emailsToDelete)
            {
                try
                {
                    await tableService.DeleteAsync("outbox", email.Id.ToString());
                    logger.LogInformation("Successfully deleted email with ID {Id}", email.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete email with ID {Id}", email.Id);
                }
            }

            logger.LogInformation("Completed cleanup of sent emails.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while cleaning up sent emails");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Clean Up Sent Email Worker is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

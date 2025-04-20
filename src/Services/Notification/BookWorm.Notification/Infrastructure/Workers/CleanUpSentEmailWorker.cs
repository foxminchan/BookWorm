using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Infrastructure.Workers;

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
            CleanUpSentEmails,
            null,
            timeUntilMidnight,
            TimeSpan.FromDays(1) // Run every 24 hours
        );

        return Task.CompletedTask;
    }

    private void CleanUpSentEmails(object? state)
    {
        try
        {
            logger.LogInformation("Starting cleanup of sent emails...");

            using var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

            // Get all sent emails
            var sentEmails = tableService.ListAsync<Outbox>("outbox").GetAwaiter().GetResult();
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
                    tableService
                        .DeleteAsync("outbox", email.Id.ToString())
                        .GetAwaiter()
                        .GetResult();
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

using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

public sealed class ResendErrorEmailWorker(
    ILogger<ResendErrorEmailWorker> logger,
    EmailOptions emailOptions,
    IServiceScopeFactory scopeFactory
) : IHostedService, IDisposable
{
    private Timer? _timer;
    private const int CheckIntervalInHours = 1;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Resend Error Email Service is starting.");

        _timer = new(
            async void (_) =>
            {
                try
                {
                    await ResendFailedEmails();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error occurred while resending failed emails");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(CheckIntervalInHours)
        );

        return Task.CompletedTask;
    }

    private async Task ResendFailedEmails()
    {
        try
        {
            logger.LogInformation("Checking for failed emails to resend...");

            using var scope = scopeFactory.CreateScope();
            var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();

            var unsentEmails = await tableService.ListAsync<Outbox>("outbox");
            var failedEmails = unsentEmails.Where(e => !e.IsSent).ToList();

            if (failedEmails.Count == 0)
            {
                logger.LogInformation("No failed emails found to resend.");
                return;
            }

            logger.LogInformation("Found {Count} failed emails to resend.", failedEmails.Count);

            foreach (var email in failedEmails)
            {
                try
                {
                    var message = OrderMimeMessageBuilder
                        .Initialize()
                        .WithTo(email.ToName, email.ToEmail)
                        .WithFrom(emailOptions)
                        .WithSubject(email.Subject)
                        .WithBody(email.Body)
                        .Build();

                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                    await sender.SendAsync(message);

                    logger.LogInformation("Successfully resent email to {Email}", email.ToEmail);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to resend email to {Email}", email.ToEmail);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing failed emails");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Resend Error Email Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

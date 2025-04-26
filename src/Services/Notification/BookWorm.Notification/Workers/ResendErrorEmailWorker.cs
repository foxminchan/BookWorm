using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

public sealed class ResendErrorEmailWorker(
    ILogger<ResendErrorEmailWorker> logger,
    EmailOptions emailOptions,
    IServiceScopeFactory scopeFactory
) : IHostedService, IDisposable
{
    private const int CheckIntervalInHours = 1;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Timer? _timer;

    public void Dispose()
    {
        _timer?.Dispose();
        _semaphore.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Resend Error Email Service is starting.");

        _timer = new(
            _ =>
                Task.Run(
                    async () =>
                    {
                        if (!await _semaphore.WaitAsync(0, cancellationToken))
                        {
                            return;
                        }

                        try
                        {
                            await ResendFailedEmails();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error occurred in timer callback");
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    },
                    cancellationToken
                ),
            null,
            TimeSpan.Zero,
            TimeSpan.FromHours(CheckIntervalInHours)
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Resend Error Email Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async Task ResendFailedEmails()
    {
        logger.LogDebug("Checking for failed emails to resend...");

        using var scope = scopeFactory.CreateScope();
        var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var unsentEmails = await tableService.ListAsync<Outbox>(nameof(Outbox).ToLower());
        var failedEmails = unsentEmails.Where(e => !e.IsSent).ToList();

        if (failedEmails.Count == 0)
        {
            logger.LogInformation("No failed emails found to resend.");
            return;
        }

        logger.LogDebug("Found {Count} failed emails to resend.", failedEmails.Count);

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

                await sender.SendAsync(message);
                logger.LogDebug("Successfully resent email to {Email}", email.ToEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to resend email to {Email}", email.ToEmail);
            }
        }
    }
}

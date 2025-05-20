using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.Workers;

[DisallowConcurrentExecution]
public sealed class ResendErrorEmailWorker(
    ILogger<ResendErrorEmailWorker> logger,
    EmailOptions emailOptions,
    IServiceScopeFactory scopeFactory
) : IJob
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await _semaphore.WaitAsync(0))
        {
            return;
        }

        try
        {
            await ResendFailedEmails();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in job execution");
        }
        finally
        {
            _semaphore.Release();
        }
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

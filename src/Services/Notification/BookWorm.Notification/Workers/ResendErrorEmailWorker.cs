using BookWorm.Notification.Domain.Models;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Notification.Workers;

[DisallowConcurrentExecution]
public sealed class ResendErrorEmailWorker(
    ILogger<ResendErrorEmailWorker> logger,
    GlobalLogBuffer logBuffer,
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
            await ResendFailedEmails(context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred in job execution");
            logBuffer.Flush();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ResendFailedEmails(CancellationToken cancellation = default)
    {
        logger.LogDebug("Checking for failed emails to resend...");

        using var scope = scopeFactory.CreateScope();
        var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var unsentEmails = await tableService.ListAsync<Outbox>(
            TablePartition.Pending,
            cancellation
        );
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
                    .WithSubject(email.Subject)
                    .WithBody(email.Body)
                    .Build();

                await sender.SendAsync(message, cancellation);
                logger.LogDebug("Successfully resent email to {Email}", email.ToEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to resend email to {Email}", email.ToEmail);
                logBuffer.Flush();
            }
        }
    }
}

using BookWorm.Notification.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Notification.Workers;

[DisallowConcurrentExecution]
public sealed class ResendErrorEmailWorker(
    ILogger<ResendErrorEmailWorker> logger,
    GlobalLogBuffer logBuffer,
    IServiceScopeFactory scopeFactory
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<INotificationDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var failedEmails = dbContext.Outboxes.Where(e => !e.IsSent).AsAsyncEnumerable();

        await foreach (var email in failedEmails.WithCancellation(context.CancellationToken))
        {
            try
            {
                var message = OrderMimeMessageBuilder
                    .Initialize()
                    .WithTo(email.ToName, email.ToEmail)
                    .WithSubject(email.Subject)
                    .WithBody(email.Body)
                    .Build();

                await sender.SendAsync(message, context.CancellationToken);
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

using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

internal sealed class ResendErrorEmailIntegrationEventHandler(
    ILogger<ResendErrorEmailIntegrationEventHandler> logger,
    GlobalLogBuffer logBuffer,
    IOutboxRepository repository,
    ISender sender
)
{
    public async Task Handle(
        ResendErrorEmailIntegrationEvent @event,
        CancellationToken cancellationToken
    )
    {
        var unsentEmails = await repository.ListAsync(new UnsentOutboxSpec(), cancellationToken);

        if (unsentEmails.Count == 0)
        {
            logger.LogDebug("No unsent emails found for resend");
            return;
        }

        var successCount = 0;
        var failureCount = 0;

        foreach (var email in unsentEmails)
        {
            try
            {
                var message = OrderMimeMessageBuilder
                    .Initialize()
                    .WithTo(email.ToName, email.ToEmail)
                    .WithSubject(email.Subject)
                    .WithBody(email.Body)
                    .Build();

                await sender.SendAsync(message, cancellationToken);

                email.MarkAsSent();
                successCount++;
                logger.LogDebug("Successfully resent email to {Email}", email.ToEmail);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                failureCount++;
                logger.LogError(ex, "Failed to resend email to {Email}", email.ToEmail);
            }
        }

        if (successCount > 0)
        {
            await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Email resend completed. Success: {SuccessCount}, Failed: {FailureCount}, Total: {TotalCount}",
            successCount,
            failureCount,
            unsentEmails.Count
        );

        if (failureCount > 0)
        {
            logBuffer.Flush();
        }
    }
}

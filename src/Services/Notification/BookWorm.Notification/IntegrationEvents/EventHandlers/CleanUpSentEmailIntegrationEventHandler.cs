using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class CleanUpSentEmailIntegrationEventHandler(
    ILogger<CleanUpSentEmailIntegrationEventHandler> logger,
    GlobalLogBuffer logBuffer,
    IOutboxRepository repository
) : IConsumer<CleanUpSentEmailIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CleanUpSentEmailIntegrationEvent> context)
    {
        try
        {
            logger.LogDebug("Starting cleanup of sent emails...");

            var sentEmails = await repository.ListAsync(
                new OutboxFilterSpec(),
                context.CancellationToken
            );

            if (sentEmails.Count != 0)
            {
                logger.LogDebug("Found {Count} sent emails to delete", sentEmails.Count);
                repository.BulkDelete(sentEmails);
                await repository.UnitOfWork.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Successfully cleaned up sent emails");
            }
            else
            {
                logger.LogDebug("No sent emails found for cleanup");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while cleaning up sent emails");
            logBuffer.Flush();
            throw new InvalidOperationException("Failed to clean up sent emails", ex);
        }
    }
}

[ExcludeFromCodeCoverage]
public sealed class CleanUpSentEmailIntegrationEventHandlerDefinition
    : ConsumerDefinition<CleanUpSentEmailIntegrationEventHandler>
{
    public CleanUpSentEmailIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-clean-up-sent-email");
        ConcurrentMessageLimit = 1;
    }
}

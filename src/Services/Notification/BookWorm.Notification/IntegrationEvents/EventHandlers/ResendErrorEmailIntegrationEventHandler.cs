using System.Collections.Concurrent;
using BookWorm.Notification.Domain.Models;

namespace BookWorm.Notification.IntegrationEvents.EventHandlers;

public sealed class ResendErrorEmailIntegrationEventHandler(
    ILogger<ResendErrorEmailIntegrationEventHandler> logger,
    GlobalLogBuffer logBuffer,
    IOutboxRepository repository,
    ISender sender
) : IConsumer<ResendErrorEmailIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ResendErrorEmailIntegrationEvent> context)
    {
        var failedEmails = await repository.ListAsync(
            new OutboxFilterSpec(),
            context.CancellationToken
        );

        var results = new ConcurrentBag<(string? email, bool success)>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = context.CancellationToken,
            MaxDegreeOfParallelism = 5,
        };

        await Parallel.ForEachAsync(
            failedEmails,
            parallelOptions,
            async (email, ct) =>
            {
                try
                {
                    var message = OrderMimeMessageBuilder
                        .Initialize()
                        .WithTo(email.ToName, email.ToEmail)
                        .WithSubject(email.Subject)
                        .WithBody(email.Body)
                        .Build();

                    await sender.SendAsync(message, ct);
                    logger.LogDebug("Successfully resent email to {Email}", email.ToEmail);
                    results.Add((email.ToEmail, true));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to resend email to {Email}", email.ToEmail);
                    results.Add((email.ToEmail, false));
                }
            }
        );

        var successCount = results.Count(r => r.success);
        var failureCount = results.Count(r => !r.success);

        logger.LogInformation(
            "Email resend completed. Success: {SuccessCount}, Failed: {FailureCount}, Total: {TotalCount}",
            successCount,
            failureCount,
            failedEmails.Count
        );

        if (failureCount > 0)
        {
            logBuffer.Flush();
        }
    }
}

[ExcludeFromCodeCoverage]
public sealed class ResendErrorEmailIntegrationEventHandlerDefinition
    : ConsumerDefinition<ResendErrorEmailIntegrationEventHandler>
{
    public ResendErrorEmailIntegrationEventHandlerDefinition()
    {
        Endpoint(x => x.Name = "notification-resend-error-email");
        ConcurrentMessageLimit = 1;
    }
}

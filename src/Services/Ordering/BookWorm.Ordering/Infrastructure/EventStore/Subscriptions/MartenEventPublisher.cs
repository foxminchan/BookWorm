using BookWorm.Chassis.OpenTelemetry;
using JasperFx.Events.Daemon;
using Marten.Subscriptions;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Ordering.Infrastructure.EventStore.Subscriptions;

public sealed class MartenEventPublisher(
    IServiceProvider serviceProvider,
    IActivityScope activityScope,
    ILogger<MartenEventPublisher> logger,
    GlobalLogBuffer logBuffer
) : SubscriptionBase
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    )
    {
        var lastProcessed = page.SequenceFloor;

        try
        {
            foreach (var @event in page.Events)
            {
                var parentContext = TelemetryPropagator.Extract(
                    @event.Headers,
                    ExtractTraceContextFromEventMetadata
                );

                await activityScope
                    .Run(
                        $"{nameof(MartenEventPublisher)}/{nameof(ProcessEventsAsync)}",
                        async (_, ct) =>
                        {
                            using var scope = serviceProvider.CreateScope();
                            var eventDispatcher =
                                scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

                            if (@event.Data is DomainEvent domainEvent)
                            {
                                await eventDispatcher
                                    .DispatchAsync(domainEvent, ct)
                                    .ConfigureAwait(false);
                            }
                        },
                        new()
                        {
                            Tags =
                            {
                                { TelemetryTags.EventHandling.Event, @event.Data.GetType().Name },
                            },
                            Parent = parentContext.ActivityContext,
                        },
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                lastProcessed = @event.Sequence;
            }

            return NullChangeListener.Instance;
        }
        catch (Exception exc)
        {
            logger.LogError(
                exc,
                "Error while processing Marten Subscription: {ExceptionMessage}",
                exc.Message
            );

            await controller.ReportCriticalFailureAsync(exc, lastProcessed).ConfigureAwait(false);

            logBuffer.Flush();

            throw new InvalidOperationException(
                $"Failed to process events in Marten subscription at sequence {lastProcessed}. See inner exception for details.",
                exc
            );
        }
    }

    private IEnumerable<string> ExtractTraceContextFromEventMetadata(
        Dictionary<string, object>? headers,
        string key
    )
    {
        try
        {
            if (headers is null || !headers.TryGetValue(key, out var value))
            {
                return [];
            }

            var stringValue = value.ToString();

            return !string.IsNullOrEmpty(stringValue) ? [stringValue] : [];
        }
        catch (Exception exc)
        {
            logger.LogError(
                exc,
                "Failed to extract trace context from event metadata for key '{Key}'",
                key
            );

            logBuffer.Flush();

            return [];
        }
    }
}

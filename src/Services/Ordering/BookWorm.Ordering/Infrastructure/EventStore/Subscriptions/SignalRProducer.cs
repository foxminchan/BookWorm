using JasperFx.Events.Daemon;
using Marten.Subscriptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.Buffering;

namespace BookWorm.Ordering.Infrastructure.EventStore.Subscriptions;

public sealed class SignalRProducer<THub>(
    IHubContext<THub> hubContext,
    ILogger<SignalRProducer<THub>> logger,
    PerRequestLogBuffer logBuffer
) : SubscriptionBase
    where THub : Hub
{
    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken
    )
    {
        foreach (var @event in page.Events)
        {
            try
            {
                await hubContext
                    .Clients.Groups(
                        $"{nameof(Stream).ToLowerInvariant()}_{@event.StreamId.ToString().GetHashCode():X}"
                    )
                    .SendAsync(
                        @event.EventTypeName,
                        @event.StreamId,
                        @event.Data,
                        @event.Data.GetType().Name,
                        cancellationToken
                    );
            }
            catch (OperationCanceledException exc)
            {
                logger.LogWarning(
                    exc,
                    "Operation cancelled while sending event {EventType} for stream {StreamId}",
                    @event.EventTypeName,
                    @event.StreamId
                );
                logBuffer.Flush();
                return NullChangeListener.Instance;
            }
            catch (HubException hubEx)
            {
                logger.LogError(
                    hubEx,
                    "SignalR hub error sending event {EventType} for stream {StreamId}",
                    @event.EventTypeName,
                    @event.StreamId
                );
                logBuffer.Flush();
                await controller.RecordDeadLetterEventAsync(@event, hubEx);
            }
            catch (Exception exc)
            {
                logger.LogError(
                    exc,
                    "Unexpected error sending event {EventType} for stream {StreamId}",
                    @event.EventTypeName,
                    @event.StreamId
                );
                logBuffer.Flush();
                await controller.RecordDeadLetterEventAsync(@event, exc);
            }
        }

        return NullChangeListener.Instance;
    }
}

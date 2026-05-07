using BookWorm.SharedKernel.SeedWork;
using Wolverine;

namespace BookWorm.Chassis.EventBus.Dispatcher;

internal sealed class EventDispatcher(IMessageBus bus, IEventMapper eventMapper) : IEventDispatcher
{
    public Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default)
    {
        return DispatchAsync(@event, null, cancellationToken);
    }

    public async Task DispatchAsync(
        DomainEvent @event,
        string? userId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(@event);

        var integrationEvent =
            eventMapper.MapToIntegrationEvent(@event)
            ?? throw new InvalidOperationException(
                $"No integration event mapping found for '{@event.GetType().Name}'."
            );

        if (string.IsNullOrEmpty(userId))
        {
            await bus.PublishAsync((object)integrationEvent);
            return;
        }

        // Propagate the user identifier through the delivery options so that the
        // envelope rule (UserIdEnvelopeMiddleware) and the CloudEvent header policy
        // pick it up when serializing the outbound Kafka message (FR-009).
        var deliveryOptions = new DeliveryOptions();
        deliveryOptions.Headers.Add(EventBusHeaders.UserId, userId);
        deliveryOptions.Headers.Add("userid", userId);

        await bus.PublishAsync((object)integrationEvent, deliveryOptions);
    }
}

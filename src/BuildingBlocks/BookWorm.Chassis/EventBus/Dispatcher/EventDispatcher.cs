using BookWorm.SharedKernel.SeedWork;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Dispatcher;

internal sealed class EventDispatcher(IBus bus, IEventMapper eventMapper) : IEventDispatcher
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
            await bus.Publish((object)integrationEvent, cancellationToken);
            return;
        }

        // Propagate the user identifier through the publishing context so that the
        // existing publishing filters (UserIdPublishFilter / KafkaPublishFilter) and
        // the CloudEvent envelope serializer pick it up just as they would for an
        // event originating from an HTTP request scope.
        await bus.Publish(
            (object)integrationEvent,
            context => context.Headers.Set(EventBusHeaders.UserId, userId),
            cancellationToken
        );
    }
}

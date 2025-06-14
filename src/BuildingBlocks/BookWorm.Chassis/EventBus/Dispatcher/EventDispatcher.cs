using BookWorm.SharedKernel.SeedWork;
using MassTransit;

namespace BookWorm.Chassis.EventBus.Dispatcher;

public sealed class EventDispatcher(IPublishEndpoint publishEndpoint, IEventMapper eventMapper)
    : IEventDispatcher
{
    public async Task DispatchAsync(
        DomainEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(@event);

        var integrationEvent =
            eventMapper.MapToIntegrationEvent(@event)
            ?? throw new InvalidOperationException(
                $"No integration event mapping found for '{@event.GetType().Name}'."
            );
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}

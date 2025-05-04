using BookWorm.SharedKernel.SeedWork;
using MassTransit;

namespace BookWorm.Chassis.EventBus;

public sealed class EventDispatcher(IPublishEndpoint publishEndpoint, IEventMapper eventMapper)
    : IEventDispatcher
{
    public async Task DispatchAsync(
        DomainEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(@event);

        var integrationEvent = eventMapper.MapToIntegrationEvent(@event);

        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}

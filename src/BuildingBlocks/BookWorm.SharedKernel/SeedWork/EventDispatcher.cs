using BookWorm.SharedKernel.SeedWork.Event;
using MassTransit;

namespace BookWorm.SharedKernel.SeedWork;

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

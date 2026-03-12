using BookWorm.SharedKernel.SeedWork;
using Wolverine;

namespace BookWorm.Chassis.EventBus.Dispatcher;

internal sealed class EventDispatcher(IMessageBus messageBus, IEventMapper eventMapper)
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
        await messageBus.PublishAsync(integrationEvent);
    }
}

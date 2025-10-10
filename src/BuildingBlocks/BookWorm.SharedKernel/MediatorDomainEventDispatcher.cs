using System.Collections.Immutable;
using BookWorm.SharedKernel.SeedWork;
using Mediator;

namespace BookWorm.SharedKernel;

public sealed class MediatorDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents)
    {
        foreach (IHasDomainEvents entity in entitiesWithEvents)
        {
            if (entity is not HasDomainEvents hasDomainEvents)
            {
                continue;
            }

            DomainEvent[] events = [.. hasDomainEvents.DomainEvents];
            hasDomainEvents.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent);
            }
        }
    }
}

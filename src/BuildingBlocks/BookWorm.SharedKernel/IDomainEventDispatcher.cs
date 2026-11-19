using System.Collections.Immutable;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.SharedKernel;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents);

    Task DispatchAndClearEvents(
        ImmutableList<IHasDomainEvents> entitiesWithEvents,
        CancellationToken cancellationToken
    )
    {
        return DispatchAndClearEvents(entitiesWithEvents);
    }
}

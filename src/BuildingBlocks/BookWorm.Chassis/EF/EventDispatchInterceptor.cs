using System.Collections.Immutable;
using BookWorm.SharedKernel;
using BookWorm.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookWorm.Chassis.EF;

internal sealed class EventDispatchInterceptor(IDomainEventDispatcher dispatcher)
    : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        var ctx = eventData.Context;

        if (ctx is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var entitiesWithEvents = ctx
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Where(static entry => entry.Entity.DomainEvents.Count != 0)
            .Select(static entry => entry.Entity)
            .ToImmutableList();

        if (!entitiesWithEvents.IsEmpty)
        {
            await dispatcher.DispatchAndClearEvents(entitiesWithEvents, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}

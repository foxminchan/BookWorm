using System.Collections.Immutable;
using BookWorm.SharedKernel;
using BookWorm.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookWorm.Chassis.EF;

public sealed class EventDispatchInterceptor(IDomainEventDispatcher dispatcher)
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
            .Select(e => e.Entity)
            .Where(x => x.DomainEvents.Count != 0)
            .ToImmutableList();

        await dispatcher.DispatchAndClearEvents(entitiesWithEvents);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}

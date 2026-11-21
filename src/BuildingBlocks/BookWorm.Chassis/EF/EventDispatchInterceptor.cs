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

        ImmutableList<IHasDomainEvents>.Builder? entitiesWithEvents = null;

        foreach (var entry in ctx.ChangeTracker.Entries<IHasDomainEvents>())
        {
            if (entry.Entity.DomainEvents.Count != 0)
            {
                (entitiesWithEvents ??= ImmutableList.CreateBuilder<IHasDomainEvents>()).Add(
                    entry.Entity
                );
            }
        }

        if (entitiesWithEvents is not null)
        {
            await dispatcher.DispatchAndClearEvents(
                entitiesWithEvents.ToImmutable(),
                cancellationToken
            );
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}

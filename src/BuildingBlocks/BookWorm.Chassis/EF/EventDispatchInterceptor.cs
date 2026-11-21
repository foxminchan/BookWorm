using System.Collections.Concurrent;
using System.Collections.Immutable;
using BookWorm.SharedKernel;
using BookWorm.SharedKernel.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookWorm.Chassis.EF;

internal sealed class EventDispatchInterceptor(IDomainEventDispatcher dispatcher)
    : SaveChangesInterceptor
{
    private readonly ConcurrentDictionary<
        DbContext,
        ImmutableList<IHasDomainEvents>
    > _pendingEntities = new();

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is { } ctx)
        {
            var entitiesWithEvents = CollectEntitiesWithEvents(ctx);
            if (!entitiesWithEvents.IsEmpty)
            {
                _pendingEntities[ctx] = entitiesWithEvents;
            }
            else
            {
                _pendingEntities.TryRemove(ctx, out _);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

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

        _pendingEntities.TryRemove(ctx, out var pendingEntities);
        var currentEntities = CollectEntitiesWithEvents(ctx);
        var entitiesWithEvents = MergeEntities(pendingEntities, currentEntities);

        if (!entitiesWithEvents.IsEmpty)
        {
            await dispatcher.DispatchAndClearEvents(entitiesWithEvents, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is { } ctx)
        {
            _pendingEntities.TryRemove(ctx, out _);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private static ImmutableList<IHasDomainEvents> CollectEntitiesWithEvents(DbContext ctx)
    {
        return ctx
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Where(static entry => entry.Entity.DomainEvents.Count != 0)
            .Select(static entry => entry.Entity)
            .ToImmutableList();
    }

    private static ImmutableList<IHasDomainEvents> MergeEntities(
        ImmutableList<IHasDomainEvents>? pending,
        ImmutableList<IHasDomainEvents> current
    )
    {
        if (pending is null or { IsEmpty: true })
        {
            return current;
        }

        if (current.IsEmpty)
        {
            return pending;
        }

        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var builder = ImmutableList.CreateBuilder<IHasDomainEvents>();

        builder.AddRange(pending.Where(seen.Add));
        builder.AddRange(current.Where(seen.Add));

        return builder.ToImmutable();
    }
}

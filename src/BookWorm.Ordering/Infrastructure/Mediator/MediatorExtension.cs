using System.Collections.Immutable;
using BookWorm.Core.SeedWork;
using BookWorm.Ordering.Infrastructure.Data;
using MediatR;

namespace BookWorm.Ordering.Infrastructure.Mediator;

public static class MediatorExtension
{
    public static async Task DispatchDomainEventsAsync(this IPublisher mediator, OrderingContext ctx)
    {
        var domainEntities = ctx.ChangeTracker
            .Entries<EntityBase>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToImmutableList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToImmutableList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}

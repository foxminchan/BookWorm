using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZLinq;

namespace BookWorm.Chassis.Mediator;

public static class Extensions
{
    public static async Task DispatchDomainEventsAsync(this IPublisher publisher, DbContext ctx)
    {
        var domainEntities = ctx
            .ChangeTracker.Entries<Entity>()
            .AsValueEnumerable()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToImmutableList();

        var domainEvents = domainEntities
            .AsValueEnumerable()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToImmutableList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}

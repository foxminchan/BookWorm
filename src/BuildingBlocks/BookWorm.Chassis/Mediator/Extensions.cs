using System.Collections.Immutable;
using BookWorm.Chassis.Pipelines;
using BookWorm.SharedKernel.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.Chassis.Mediator;

public static class Extensions
{
    public static void AddMediatR<TMarker>(
        this IServiceCollection services,
        Action<MediatRServiceConfiguration>? configuration = null
    )
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<TMarker>();
            config.AddOpenBehavior(typeof(ActivityBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration?.Invoke(config);
        });
    }

    public static async Task DispatchDomainEventsAsync(this IPublisher publisher, DbContext ctx)
    {
        var domainEntities = ctx
            .ChangeTracker.Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Count != 0)
            .ToImmutableList();

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToImmutableList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}

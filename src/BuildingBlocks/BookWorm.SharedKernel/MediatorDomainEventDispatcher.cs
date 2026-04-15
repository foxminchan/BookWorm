using System.Collections.Immutable;
using BookWorm.SharedKernel.SeedWork;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace BookWorm.SharedKernel;

internal sealed class MediatorDomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
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

public static class MediatorDomainEventDispatcherExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the <see cref="MediatorDomainEventDispatcher" /> as a scoped implementation
        ///     of <see cref="IDomainEventDispatcher" /> in the dependency injection container.
        /// </summary>
        public void AddMediatorDomainEventDispatcher()
        {
            services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
        }
    }
}

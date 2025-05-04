using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.EventBus;

public interface IEventDispatcher
{
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}

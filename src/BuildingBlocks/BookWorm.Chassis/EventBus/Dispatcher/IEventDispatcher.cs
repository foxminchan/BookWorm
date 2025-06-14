using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.EventBus.Dispatcher;

public interface IEventDispatcher
{
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}

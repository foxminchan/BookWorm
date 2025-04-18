using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.SharedKernel.EventBus;

public interface IEventDispatcher
{
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}

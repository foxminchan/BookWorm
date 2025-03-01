using BookWorm.SharedKernel.SeedWork.Event;

namespace BookWorm.SharedKernel.SeedWork;

public interface IEventDispatcher
{
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}

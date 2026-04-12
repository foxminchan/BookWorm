using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chassis.EventBus.Dispatcher;

public interface IEventDispatcher
{
    /// <summary>
    ///     Dispatches the specified domain event to its registered handlers.
    /// </summary>
    /// <param name="event">The domain event instance to dispatch.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous dispatch operation.</param>
    /// <returns>A task that represents the asynchronous dispatch operation.</returns>
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}

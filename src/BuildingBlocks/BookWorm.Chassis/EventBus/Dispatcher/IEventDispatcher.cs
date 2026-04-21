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

    /// <summary>
    ///     Dispatches the specified domain event to its registered handlers, propagating
    ///     the originating user identifier through the publish context headers so that
    ///     it is preserved across asynchronous boundaries (e.g. Marten projections,
    ///     background daemons) where the original <c>HttpContext</c> is unavailable.
    /// </summary>
    /// <param name="event">The domain event instance to dispatch.</param>
    /// <param name="userId">
    ///     The user identifier to attach to the resulting integration event headers.
    ///     When <see langword="null" /> or empty, no user header is set and the call
    ///     behaves like <see cref="DispatchAsync(DomainEvent, CancellationToken)" />.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous dispatch operation.</param>
    /// <returns>A task that represents the asynchronous dispatch operation.</returns>
    Task DispatchAsync(
        DomainEvent @event,
        string? userId,
        CancellationToken cancellationToken = default
    );
}

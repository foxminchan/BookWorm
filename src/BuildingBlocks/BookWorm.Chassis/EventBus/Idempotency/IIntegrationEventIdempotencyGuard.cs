namespace BookWorm.Chassis.EventBus.Idempotency;

public interface IIntegrationEventIdempotencyGuard
{
    /// <summary>
    ///     Attempts to acquire a lock for the given event ID and consumer name.
    ///     If the lock is acquired, it will be held for the specified TTL (time-to-live) duration.
    /// </summary>
    /// <param name="eventId">The unique identifier of the integration event.</param>
    /// <param name="consumerName">The name of the consumer attempting to acquire the lock.</param>
    /// <param name="ttl">The optional time-to-live duration for the lock. If not specified, a default TTL will be used.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the lock acquisition.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains a boolean indicating whether the lock was
    ///     successfully acquired (true) or if it was already held by another consumer (false).
    /// </returns>
    Task<bool> TryAcquireAsync(
        Guid eventId,
        string consumerName,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Releases a previously acquired idempotency lock when event handling fails transiently,
    ///     allowing the message broker retry policy to reprocess the event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the integration event.</param>
    /// <param name="consumerName">The name of the consumer releasing the lock.</param>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    async Task ReleaseAsync(
        Guid eventId,
        string consumerName,
        CancellationToken cancellationToken = default
    )
    {
        await Task.CompletedTask;
    }
}

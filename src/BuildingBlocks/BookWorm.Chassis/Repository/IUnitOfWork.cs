namespace BookWorm.Chassis.Repository;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     Persists all tracked changes to the underlying data store.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves entities and dispatches related domain events as part of the unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true" /> when the operation succeeds; otherwise, <see langword="false" />.</returns>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}

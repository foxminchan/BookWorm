namespace BookWorm.Notification.Infrastructure.Table;

public interface ITableService
{
    Task<Guid> UpsertAsync<T>(
        T entity,
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task<IEnumerable<T>> ListAsync<T>(
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class;

    Task DeleteAsync(
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default
    );

    Task BulkDeleteAsync(string partitionKey, CancellationToken cancellationToken = default);
}

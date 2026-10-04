using Refit;

namespace BookWorm.Scheduler.Clients;

public interface ICatalogIngestionApi
{
    [Post("/api/vectors/batch")]
    Task ProcessVectorBatchAsync(CancellationToken cancellationToken = default);
}

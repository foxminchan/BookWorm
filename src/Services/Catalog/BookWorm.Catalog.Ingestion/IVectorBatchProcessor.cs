namespace BookWorm.Catalog.Ingestion;

public interface IVectorBatchProcessor
{
    Task<int> ProcessAsync(CancellationToken cancellationToken);
}

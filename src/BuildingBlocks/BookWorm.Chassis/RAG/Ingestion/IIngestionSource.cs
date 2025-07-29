namespace BookWorm.Chassis.RAG.Ingestion;

public interface IIngestionSource<in T>
    where T : class
{
    Task IngestDataAsync(T data, CancellationToken cancellationToken = default);
}

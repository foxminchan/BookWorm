using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi.Ingestion;

public sealed class BookDataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore
) : IIngestionSource<Book>
{
    private readonly string _collectionName = nameof(Book).ToLowerInvariant();

    public async Task IngestDataAsync(Book data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(data.Name);
        ArgumentException.ThrowIfNullOrEmpty(data.Description);

        var vectorCollection = vectorStore.GetCollection<Guid, HybridSearchRecord>(_collectionName);
        await vectorCollection.CreateCollectionIfNotExistsAsync(cancellationToken);

        var text = $"{data.Name} {data.Description}";

        var embeddings = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var record = new HybridSearchRecord
        {
            Id = data.Id,
            Description = text,
            Vector = embeddings,
        };

        await vectorCollection.UpsertAsync(record, cancellationToken);
    }
}

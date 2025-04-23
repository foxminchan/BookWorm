using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi.Ingestion;

public sealed class BookDataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore
) : IIngestionSource<Book>
{
    private readonly string _collectionName = nameof(Book).ToLower();

    public async Task IngestDataAsync(Book data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(data.Name);
        ArgumentException.ThrowIfNullOrEmpty(data.Description);

        var vectorCollection = vectorStore.GetCollection<Guid, SemanticSearchRecord>(
            _collectionName
        );
        await vectorCollection.CreateCollectionIfNotExistsAsync(cancellationToken);

        var embeddings = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            $"{data.Name} {data.Description}",
            cancellationToken: cancellationToken
        );

        var record = new SemanticSearchRecord
        {
            Id = data.Id,
            Name = data.Name,
            Vector = embeddings,
        };

        await vectorCollection.UpsertAsync(record, cancellationToken);
    }
}

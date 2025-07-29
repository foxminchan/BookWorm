using BookWorm.Chassis.RAG.Ingestion;
using BookWorm.Chassis.RAG.Search;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi.Ingestion;

public sealed class BookDataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, TextSnippet> vectorCollection
) : IIngestionSource<Book>
{
    public async Task IngestDataAsync(Book data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(data.Name);
        ArgumentException.ThrowIfNullOrEmpty(data.Description);

        await vectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        var text = $"{data.Name} {data.Description}";

        var embeddings = await embeddingGenerator.GenerateVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var record = new TextSnippet
        {
            Id = data.Id,
            Description = text,
            Vector = embeddings,
        };

        await vectorCollection.UpsertAsync(record, cancellationToken);
    }
}

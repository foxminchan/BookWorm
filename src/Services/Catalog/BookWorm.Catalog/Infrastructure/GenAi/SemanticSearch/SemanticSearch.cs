using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi.SemanticSearch;

public sealed class SemanticSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore
) : ISemanticSearch
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> FindAsync(
        string text,
        string collectionName,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    )
    {
        var queryEmbedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var vectorCollection = vectorStore.GetCollection<Guid, SemanticSearchRecord>(
            collectionName
        );

        var nearest = await vectorCollection.VectorizedSearchAsync(
            queryEmbedding,
            new() { Top = maxResults },
            cancellationToken
        );

        var results = new List<SemanticSearchRecord>();

        await foreach (var item in nearest.Results.WithCancellation(cancellationToken))
        {
            results.Add(item.Record);
        }

        return results;
    }
}

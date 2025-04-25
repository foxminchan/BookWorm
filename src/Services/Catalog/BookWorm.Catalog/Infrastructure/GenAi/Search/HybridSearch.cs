using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi.Search;

public sealed class HybridSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore
) : ISearch
{
    public async Task<IReadOnlyList<HybridSearchRecord>> SearchAsync(
        string text,
        ICollection<string> keywords,
        string collectionName,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    )
    {
        var vector = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var vectorCollection =
            (IKeywordHybridSearch<HybridSearchRecord>)
                vectorStore.GetCollection<Guid, HybridSearchRecord>(collectionName);

        var options = new HybridSearchOptions<HybridSearchRecord>
        {
            Top = maxResults,
            VectorProperty = r => r.Vector,
            AdditionalProperty = r => r.Description,
        };

        var nearest = await vectorCollection.HybridSearchAsync(
            vector,
            keywords,
            options,
            cancellationToken
        );

        var results = new List<HybridSearchRecord>();

        await foreach (var item in nearest.Results.WithCancellation(cancellationToken))
        {
            results.Add(item.Record);
        }

        return results;
    }
}

using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.Search;

public sealed class HybridSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore
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
        var vector = await embeddingGenerator.GenerateVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var vectorCollection =
            (IKeywordHybridSearchable<HybridSearchRecord>)
                vectorStore.GetCollection<Guid, HybridSearchRecord>(collectionName);

        var options = new HybridSearchOptions<HybridSearchRecord>
        {
            VectorProperty = r => r.Vector,
            AdditionalProperty = r => r.Description,
        };

        var nearest = vectorCollection.HybridSearchAsync(
            vector,
            keywords,
            maxResults,
            options,
            cancellationToken
        );

        List<HybridSearchRecord> results = [];

        await foreach (var item in nearest)
        {
            results.Add(item.Record);
        }

        return results;
    }
}

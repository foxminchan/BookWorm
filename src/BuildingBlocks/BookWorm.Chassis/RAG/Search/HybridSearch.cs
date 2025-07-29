using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.RAG.Search;

public sealed class HybridSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, TextSnippet> collection
) : ISearch
{
    public async Task<IReadOnlyList<TextSnippet>> SearchAsync(
        string text,
        ICollection<string> keywords,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    )
    {
        var vector = await embeddingGenerator.GenerateVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        await collection.EnsureCollectionExistsAsync(cancellationToken);
        var vectorCollection = (IKeywordHybridSearchable<TextSnippet>)collection;

        var options = new HybridSearchOptions<TextSnippet>
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

        List<TextSnippet> results = [];

        await foreach (var item in nearest)
        {
            results.Add(item.Record);
        }

        return results;
    }
}

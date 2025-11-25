using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.AI.Search;

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
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var vectorCollection = (IKeywordHybridSearchable<TextSnippet>)collection;

        var vector = await embeddingGenerator.GenerateVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        var options = new HybridSearchOptions<TextSnippet>
        {
            VectorProperty = r => r.Vector,
            AdditionalProperty = r => r.Content,
        };

        var nearest = vectorCollection.HybridSearchAsync(
            vector,
            keywords,
            maxResults,
            options,
            cancellationToken
        );

        return await nearest.Select(result => result.Record).ToListAsync(cancellationToken);
    }
}

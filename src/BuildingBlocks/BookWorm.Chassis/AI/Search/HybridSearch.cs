using System.Collections.Concurrent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.AI.Search;

internal sealed class HybridSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, TextSnippet> collection
) : ISearch
{
    private static readonly ConcurrentDictionary<string, byte> _ensuredCollections = new(
        StringComparer.Ordinal
    );

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _collectionLocks = new(
        StringComparer.Ordinal
    );

    public async Task<IReadOnlyList<TextSnippet>> SearchAsync(
        string text,
        ICollection<string> keywords,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureCollectionExistsOnceAsync(cancellationToken);

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

    private async Task EnsureCollectionExistsOnceAsync(CancellationToken cancellationToken)
    {
        var collectionName = collection.Name;

        if (_ensuredCollections.ContainsKey(collectionName))
        {
            return;
        }

        var gate = _collectionLocks.GetOrAdd(collectionName, static _ => new(1, 1));
        await gate.WaitAsync(cancellationToken);

        try
        {
            if (_ensuredCollections.ContainsKey(collectionName))
            {
                return;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);
            _ensuredCollections.TryAdd(collectionName, 0);
        }
        finally
        {
            gate.Release();
        }
    }
}

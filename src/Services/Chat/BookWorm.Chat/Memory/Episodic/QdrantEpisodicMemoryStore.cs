using System.Linq.Expressions;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.Memory.Episodic;

/// <summary>
///     Qdrant-backed <see cref="IEpisodicMemoryStore" /> over the <c>bookagent-episodes</c> collection.
/// </summary>
internal sealed class QdrantEpisodicMemoryStore(
    VectorStoreCollection<Guid, EpisodicMemory> collection,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    UserMemoryOptions options,
    TimeProvider timeProvider
) : IEpisodicMemoryStore
{
    private const int BatchSize = 100;

    // Episodes lose half of their recency boost every 30 days; the boost only breaks near-ties
    // in similarity and never outweighs relevance.
    private const double RecencyWeight = 0.1;
    private static readonly TimeSpan _recencyHalfLife = TimeSpan.FromDays(30);

    public async Task<IReadOnlyList<RememberedEpisode>> SearchAsync(
        string userId,
        string query,
        int maxResults,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (string.IsNullOrWhiteSpace(query) || maxResults <= 0)
        {
            return [];
        }

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var vector = await embeddingGenerator.GenerateVectorAsync(
            query,
            cancellationToken: cancellationToken
        );

        var now = timeProvider.GetUtcNow();

        var searchOptions = new VectorSearchOptions<EpisodicMemory>
        {
            Filter = CreateUserFilter(userId, now),
        };

        // Over-fetch so that recency re-ranking can promote a slightly less similar but recent episode.
        var results = await collection
            .SearchAsync(vector, maxResults * 2, searchOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        return
        [
            .. results
                // Defense in depth: never surface an episode that does not belong to the user.
                .Where(r => r.Record.UserId == userId && r.Record.ExpiresAt > now)
                .Where(r => (r.Score ?? 0d) >= options.MinEpisodeRelevance)
                .Select(r =>
                    (
                        Result: r,
                        Rank: (r.Score ?? 0d)
                            + RecencyWeight
                                * Math.Pow(0.5, (now - r.Record.CreatedAt) / _recencyHalfLife)
                    )
                )
                .OrderByDescending(x => x.Rank)
                .Take(maxResults)
                .Select(x => ToRemembered(x.Result.Record, x.Result.Score)),
        ];
    }

    public async Task<IReadOnlyList<RememberedEpisode>> ListAsync(
        string userId,
        int maxResults,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();

        var records = await collection
            .GetAsync(
                CreateUserFilter(userId, now),
                maxResults,
                cancellationToken: cancellationToken
            )
            .ToListAsync(cancellationToken);

        return
        [
            .. records
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => ToRemembered(r, null)),
        ];
    }

    public async Task AddAsync(
        string userId,
        string summary,
        IReadOnlyCollection<string> bookIds,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var vector = await embeddingGenerator.GenerateVectorAsync(
            summary,
            cancellationToken: cancellationToken
        );

        var now = timeProvider.GetUtcNow();

        await collection.UpsertAsync(
            new EpisodicMemory
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Summary = summary,
                BookIds = [.. bookIds],
                CreatedAt = now,
                ExpiresAt = now.Add(options.EpisodeRetention),
                Vector = vector,
            },
            cancellationToken
        );
    }

    public async Task<bool> DeleteAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var record = await collection.GetAsync(id, cancellationToken: cancellationToken);

        if (record is null || record.UserId != userId)
        {
            return false;
        }

        await collection.DeleteAsync(id, cancellationToken);

        return true;
    }

    public async Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await DeleteWhereAsync(r => r.UserId == userId, cancellationToken);
    }

    public Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        return DeleteWhereAsync(r => r.ExpiresAt <= now, cancellationToken);
    }

    private static Expression<Func<EpisodicMemory, bool>> CreateUserFilter(
        string userId,
        DateTimeOffset now
    )
    {
        return r => r.UserId == userId && r.ExpiresAt > now;
    }

    private async Task<int> DeleteWhereAsync(
        Expression<Func<EpisodicMemory, bool>> filter,
        CancellationToken cancellationToken
    )
    {
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var total = 0;

        while (true)
        {
            var keys = await collection
                .GetAsync(filter, BatchSize, cancellationToken: cancellationToken)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (keys.Count == 0)
            {
                return total;
            }

            await collection.DeleteAsync(keys, cancellationToken);
            total += keys.Count;

            if (keys.Count < BatchSize)
            {
                return total;
            }
        }
    }

    private static RememberedEpisode ToRemembered(EpisodicMemory record, double? relevance)
    {
        return new(record.Id, record.Summary, record.BookIds, record.CreatedAt, relevance);
    }
}

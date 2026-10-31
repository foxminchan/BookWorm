using System.Text.Json.Serialization;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chat.Memory.Episodic;

/// <summary>
///     A user-scoped summary of a specific past interaction (episodic memory), stored in Qdrant.
/// </summary>
/// <remarks>
///     Episodes are append-only and carry an <see cref="ExpiresAt" /> timestamp that acts as a TTL:
///     expired episodes are never recalled and are periodically purged.
/// </remarks>
internal sealed class EpisodicMemory
{
    // Matches the Cohere Embed v4 deployment used for catalog snippets (1536 dimensions).
    private const int VectorDimensions = 1536;
    public const string CollectionName = "bookagent-episodes";

    [VectorStoreKey(StorageName = "key")]
    [JsonPropertyName("key")]
    public required Guid Id { get; init; }

    [VectorStoreData(IsIndexed = true, StorageName = "user_id")]
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [VectorStoreData(StorageName = "summary")]
    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    [VectorStoreData(StorageName = "book_ids")]
    [JsonPropertyName("book_ids")]
    public string[] BookIds { get; init; } = [];

    [VectorStoreData(IsIndexed = true, StorageName = "created_at")]
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [VectorStoreData(IsIndexed = true, StorageName = "expires_at")]
    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; init; }

    [VectorStoreVector(
        VectorDimensions,
        DistanceFunction = DistanceFunction.CosineSimilarity,
        StorageName = "embedding"
    )]
    [JsonPropertyName("embedding")]
    public ReadOnlyMemory<float> Vector { get; init; }
}

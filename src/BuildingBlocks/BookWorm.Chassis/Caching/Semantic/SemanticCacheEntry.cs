using System.Text.Json.Serialization;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.Caching.Semantic;

public sealed class SemanticCacheEntry
{
    // Cohere Embed v4 returns 1536-dimensional vectors by default.
    private const int VectorDimensions = 1536;
    private const string VectorDistanceFunction = DistanceFunction.CosineSimilarity;
    public const string CollectionName = "data-bookworm-semantic-cache";

    [VectorStoreKey(StorageName = "key")]
    [JsonPropertyName("key")]
    public required string Id { get; init; }

    [VectorStoreData(StorageName = "prompt")]
    [JsonPropertyName("prompt")]
    public required string Prompt { get; init; }

    [VectorStoreData(StorageName = "response_text")]
    [JsonPropertyName("response_text")]
    public required string ResponseText { get; init; }

    [VectorStoreData(StorageName = "model_id")]
    [JsonPropertyName("model_id")]
    public string? ModelId { get; init; }

    [VectorStoreData(StorageName = "expires_at_unix_seconds")]
    [JsonPropertyName("expires_at_unix_seconds")]
    public long ExpiresAtUnixSeconds { get; init; }

    [VectorStoreVector(
        VectorDimensions,
        DistanceFunction = VectorDistanceFunction,
        StorageName = "embedding"
    )]
    [JsonPropertyName("embedding")]
    public ReadOnlyMemory<float> Vector { get; init; }
}

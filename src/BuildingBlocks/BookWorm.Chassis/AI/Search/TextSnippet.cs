using System.Text.Json.Serialization;
using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.AI.Search;

public sealed class TextSnippet
{
    // 1536 is the default vector size for the OpenAI text-embedding-3-small model
    private const int VectorDimensions = 1536;
    private const string VectorDistanceFunction = DistanceFunction.CosineSimilarity;
    public const string CollectionName = "data-bookworm-snippets";

    [VectorStoreKey]
    [JsonPropertyName("key")]
    public required Guid Id { get; init; }

    [VectorStoreData(IsFullTextIndexed = true, StorageName = "content")]
    [JsonPropertyName("content")]
    public required string Content { get; init; }
    
    [VectorStoreData(StorageName = "context")]
    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [VectorStoreVector(VectorDimensions, DistanceFunction = VectorDistanceFunction, StorageName = "embedding")]
    [JsonPropertyName("embedding")]
    public ReadOnlyMemory<float> Vector { get; init; }
}

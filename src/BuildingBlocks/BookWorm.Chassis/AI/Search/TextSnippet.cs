using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.AI.Search;

public sealed class TextSnippet
{
    [VectorStoreKey]
    public required Guid Id { get; init; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string? Description { get; init; }

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; init; }
}

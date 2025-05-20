using Microsoft.Extensions.VectorData;

namespace BookWorm.Chassis.Search;

public sealed class HybridSearchRecord
{
    [VectorStoreKey]
    public required Guid Id { get; init; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public required string Description { get; init; }

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; init; }
}

using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi;

public sealed class SemanticSearchRecord
{
    [VectorStoreRecordKey]
    public required Guid Id { get; init; }

    [VectorStoreRecordData]
    public required string Name { get; set; }

    [VectorStoreRecordVector(768, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

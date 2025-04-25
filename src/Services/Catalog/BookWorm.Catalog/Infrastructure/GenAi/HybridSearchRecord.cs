using Microsoft.Extensions.VectorData;

namespace BookWorm.Catalog.Infrastructure.GenAi;

public sealed class HybridSearchRecord
{
    [VectorStoreRecordKey]
    public required Guid Id { get; init; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string Description { get; set; }

    [VectorStoreRecordVector(768, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

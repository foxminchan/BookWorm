using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace BookWorm.Chassis.Search;

public sealed class TextSnippet
{
    [VectorStoreKey]
    public required Guid Id { get; init; }

    [TextSearchResultValue]
    [VectorStoreData(IsFullTextIndexed = true)]
    public string? Description { get; init; }

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; init; }
}

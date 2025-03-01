using Qdrant.Client;

namespace BookWorm.Catalog.Infrastructure.Services;

public sealed class BookSemanticSearch(IAiService aiService, QdrantClient qdrantClient)
    : IBookSemanticSearch
{
    private static readonly string _collection = nameof(Book).ToLowerInvariant();

    public async Task<Guid[]> FindBooksAsync(
        string searchText,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<Guid>();

        var embeddings = await aiService.GetEmbeddingAsync(searchText, cancellationToken);

        var scoredPoints = await qdrantClient.SearchAsync(
            _collection,
            embeddings.ToArray(),
            cancellationToken: cancellationToken
        );

        results.AddRange(scoredPoints.Select(scoredPoint => Guid.Parse(scoredPoint.Id.Uuid)));

        return [.. results];
    }
}

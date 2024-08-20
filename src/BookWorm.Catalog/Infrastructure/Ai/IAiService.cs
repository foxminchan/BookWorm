namespace BookWorm.Catalog.Infrastructure.Ai;

public interface IAiService
{
    ValueTask<Vector> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(List<string> text,
        CancellationToken cancellationToken = default);
}

namespace BookWorm.Catalog.Infrastructure.AI;

public interface IAiService
{
    ValueTask<ReadOnlyMemory<float>> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default
    );
}

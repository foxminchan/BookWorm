using Microsoft.Extensions.AI;

namespace BookWorm.Catalog.Infrastructure.Ai;

public sealed class AiService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<AiService> logger
) : IAiService
{
    private const int EmbeddingDimensions = 384;

    public async ValueTask<Vector> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        var timestamp = Stopwatch.GetTimestamp();

        var embedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        embedding = embedding[0..EmbeddingDimensions];

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Generated embedding in {ElapsedMilliseconds}s: '{Text}'",
                Stopwatch.GetElapsedTime(timestamp).TotalSeconds,
                text
            );
        }

        return new(embedding);
    }

    public async ValueTask<IReadOnlyList<Vector>> GetEmbeddingsAsync(
        List<string> text,
        CancellationToken cancellationToken = default
    )
    {
        var timestamp = Stopwatch.GetTimestamp();

        var embeddings = await embeddingGenerator.GenerateAsync(
            text,
            cancellationToken: cancellationToken
        );

        var results = embeddings.Select(m => new Vector(m.Vector[0..EmbeddingDimensions])).ToList();

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Generated {EmbeddingsCount} embeddings in {ElapsedMilliseconds}s",
                results.Count,
                Stopwatch.GetElapsedTime(timestamp).TotalSeconds
            );
        }

        return results;
    }
}

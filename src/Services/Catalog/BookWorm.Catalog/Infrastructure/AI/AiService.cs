using System.Diagnostics;
using Microsoft.Extensions.AI;

namespace BookWorm.Catalog.Infrastructure.AI;

public sealed class AiService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ILogger<AiService> logger
) : IAiService
{
    private const int EmbeddingDimensions = 768;

    public async ValueTask<ReadOnlyMemory<float>> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default
    )
    {
        var timestamp = Stopwatch.GetTimestamp();

        var embedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(
            text,
            cancellationToken: cancellationToken
        );

        embedding = embedding[..EmbeddingDimensions];

        if (logger.IsEnabled(LogLevel.Trace))
        {
            logger.LogTrace(
                "Generated embedding in {ElapsedMilliseconds}s: '{Text}'",
                Stopwatch.GetElapsedTime(timestamp).TotalSeconds,
                text
            );
        }

        return embedding;
    }
}

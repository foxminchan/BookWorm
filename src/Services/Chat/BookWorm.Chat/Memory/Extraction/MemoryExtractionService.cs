using BookWorm.Chat.Memory.Episodic;
using BookWorm.Chat.Memory.Semantic;

namespace BookWorm.Chat.Memory.Extraction;

/// <summary>
///     Background consumer that turns queued exchanges into episodic and semantic memories, so that
///     extraction (Presidio + LLM + stores) never blocks the AG-UI stream.
/// </summary>
internal sealed class MemoryExtractionService(
    IMemoryExtractionQueue queue,
    IMemoryExtractor extractor,
    IEpisodicMemoryStore episodicStore,
    IServiceScopeFactory scopeFactory,
    ILogger<MemoryExtractionService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(request, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // A failed extraction only loses one memory candidate; keep consuming.
                logger.LogWarning(ex, "Failed to extract memory from a BookAgent exchange");
            }
        }
    }

    /// <summary>
    ///     Extracts and persists the memories for a single exchange.
    /// </summary>
    internal async Task ProcessAsync(
        MemoryExtractionRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await extractor.ExtractAsync(request, cancellationToken);

        if (!string.IsNullOrWhiteSpace(result.EpisodeSummary))
        {
            await episodicStore.AddAsync(
                request.UserId,
                result.EpisodeSummary,
                result.BookIds,
                cancellationToken
            );
        }

        if (result.Preferences.Count != 0)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var preferenceStore = scope.ServiceProvider.GetRequiredService<IPreferenceStore>();

            await preferenceStore.ApplyAsync(request.UserId, result.Preferences, cancellationToken);
        }
    }
}

namespace BookWorm.Chat.Memory.Episodic;

/// <summary>
///     Periodically purges expired episodic memories. Expired episodes are already excluded from recall;
///     this sweep enforces the retention period on the stored data itself.
/// </summary>
internal sealed class EpisodicMemoryRetentionService(
    IEpisodicMemoryStore episodicStore,
    UserMemoryOptions options,
    TimeProvider timeProvider,
    ILogger<EpisodicMemoryRetentionService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.RetentionSweepInterval, timeProvider);

        try
        {
            do
            {
                try
                {
                    var purged = await episodicStore.PurgeExpiredAsync(stoppingToken);

                    if (purged > 0)
                    {
                        logger.LogInformation("Purged {Count} expired episodic memories", purged);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Failed to purge expired episodic memories");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is shutting down.
        }
    }
}

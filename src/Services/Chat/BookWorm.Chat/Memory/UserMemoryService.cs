using BookWorm.Chat.Memory.Episodic;
using BookWorm.Chat.Memory.Semantic;

namespace BookWorm.Chat.Memory;

/// <summary>
///     Default <see cref="IUserMemoryService" />. Registered as a singleton (it is used by the singleton
///     BookAgent), so the scoped <see cref="IPreferenceStore" /> is resolved per operation.
/// </summary>
internal sealed class UserMemoryService(
    IServiceScopeFactory scopeFactory,
    IEpisodicMemoryStore episodicStore,
    UserMemoryOptions options,
    ILogger<UserMemoryService> logger
) : IUserMemoryService
{
    // Upper bound for the "view my memory" listing.
    private const int MaxListedEpisodes = 100;

    public async Task<UserMemorySnapshot> RecallAsync(
        string userId,
        string? query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var preferencesTask = RecallPreferencesAsync(userId, cancellationToken);
        var episodesTask = RecallEpisodesAsync(userId, query, cancellationToken);

        await Task.WhenAll(preferencesTask, episodesTask);

        var preferences = await preferencesTask;
        var episodes = await episodesTask;

        return preferences.Count == 0 && episodes.Count == 0
            ? UserMemorySnapshot.Empty
            : new(preferences, episodes);
    }

    public async Task<UserMemorySnapshot> GetAllAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var scope = scopeFactory.CreateAsyncScope();
        var preferenceStore = scope.ServiceProvider.GetRequiredService<IPreferenceStore>();

        var preferences = await preferenceStore.ListAsync(userId, cancellationToken);
        var episodes = await episodicStore.ListAsync(userId, MaxListedEpisodes, cancellationToken);

        return new(preferences, episodes);
    }

    public async Task ForgetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var scope = scopeFactory.CreateAsyncScope();
        var preferenceStore = scope.ServiceProvider.GetRequiredService<IPreferenceStore>();

        await preferenceStore.DeleteAllAsync(userId, cancellationToken);
        await episodicStore.DeleteAllAsync(userId, cancellationToken);
    }

    public async Task<bool> ForgetPreferenceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var scope = scopeFactory.CreateAsyncScope();
        var preferenceStore = scope.ServiceProvider.GetRequiredService<IPreferenceStore>();

        return await preferenceStore.DeleteAsync(userId, id, cancellationToken);
    }

    public Task<bool> ForgetEpisodeAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        return episodicStore.DeleteAsync(userId, id, cancellationToken);
    }

    private async Task<IReadOnlyList<RememberedPreference>> RecallPreferencesAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var preferenceStore = scope.ServiceProvider.GetRequiredService<IPreferenceStore>();

            return await preferenceStore.GetRelevantAsync(
                userId,
                options.MaxPreferences,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to recall semantic memory; continuing without it");
            return [];
        }
    }

    private async Task<IReadOnlyList<RememberedEpisode>> RecallEpisodesAsync(
        string userId,
        string? query,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        try
        {
            return await episodicStore.SearchAsync(
                userId,
                query,
                options.MaxEpisodes,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to recall episodic memory; continuing without it");
            return [];
        }
    }
}

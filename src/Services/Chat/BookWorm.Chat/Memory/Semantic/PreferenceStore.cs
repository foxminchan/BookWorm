using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     PostgreSQL-backed <see cref="IPreferenceStore" /> over the <c>user_preferences</c> table.
/// </summary>
internal sealed class PreferenceStore(
    ChatDbContext dbContext,
    UserMemoryOptions options,
    TimeProvider timeProvider
) : IPreferenceStore
{
    public async Task<IReadOnlyList<RememberedPreference>> GetRelevantAsync(
        string userId,
        int maxResults,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var preferences = await dbContext
            .UserPreferences.Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        // Per-user preferences are bounded by pruning, so ranking in memory is cheap and keeps
        // the decay formula in a single place.
        return
        [
            .. preferences
                .Select(p => ToRemembered(p, now))
                .Where(p => p.Confidence >= options.MinPreferenceConfidence)
                .OrderByDescending(p => p.Confidence)
                .ThenByDescending(p => p.LastSeenAt)
                .Take(maxResults),
        ];
    }

    public async Task<IReadOnlyList<RememberedPreference>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var preferences = await dbContext
            .UserPreferences.Where(p => p.UserId == userId)
            .OrderByDescending(p => p.LastSeenAt)
            .ToListAsync(cancellationToken);

        return [.. preferences.Select(p => ToRemembered(p, now))];
    }

    public async Task ApplyAsync(
        string userId,
        IReadOnlyCollection<PreferenceCandidate> candidates,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (candidates.Count == 0)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        var existing = await dbContext
            .UserPreferences.AsTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            var normalized = PreferenceConsolidator.Normalize(candidate.Value);

            var match = existing.FirstOrDefault(p =>
                p.Kind == candidate.Kind && p.NormalizedValue == normalized
            );

            var merged = PreferenceConsolidator.Merge(
                match,
                userId,
                candidate,
                now,
                options.PreferenceHalfLife
            );

            if (match is null)
            {
                dbContext.UserPreferences.Add(merged);
                existing.Add(merged);
            }
        }

        // Retention: drop preferences that decayed below the threshold and cap the per-user total.
        var ranked = existing
            .Select(p =>
                (
                    Preference: p,
                    Confidence: PreferenceConsolidator.GetEffectiveConfidence(
                        p,
                        now,
                        options.PreferenceHalfLife
                    )
                )
            )
            .OrderByDescending(x => x.Confidence)
            .ToList();

        var stale = ranked
            .Where(
                (x, index) =>
                    x.Confidence < options.MinPreferenceConfidence
                    || index >= options.MaxPreferencesPerUser
            )
            .Select(x => x.Preference)
            .ToList();

        if (stale.Count != 0)
        {
            dbContext.UserPreferences.RemoveRange(stale);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var deleted = await dbContext
            .UserPreferences.Where(p => p.UserId == userId && p.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }

    public async Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await dbContext
            .UserPreferences.Where(p => p.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private RememberedPreference ToRemembered(UserPreference preference, DateTime now)
    {
        return new(
            preference.Id,
            preference.Kind,
            preference.Value,
            preference.Sentiment,
            PreferenceConsolidator.GetEffectiveConfidence(
                preference,
                now,
                options.PreferenceHalfLife
            ),
            preference.EvidenceCount,
            preference.LastSeenAt
        );
    }
}

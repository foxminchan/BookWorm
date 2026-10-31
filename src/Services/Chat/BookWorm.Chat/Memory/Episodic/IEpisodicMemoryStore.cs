namespace BookWorm.Chat.Memory.Episodic;

/// <summary>
///     Durable, user-scoped storage for episodic memories. Every read and delete is filtered by the
///     owning user id at the vector-store level, so one user's episodes are never visible to another.
/// </summary>
internal interface IEpisodicMemoryStore
{
    /// <summary>
    ///     Finds the user's unexpired episodes most relevant to <paramref name="query" />,
    ///     ranked by similarity with a small recency boost.
    /// </summary>
    Task<IReadOnlyList<RememberedEpisode>> SearchAsync(
        string userId,
        string query,
        int maxResults,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets the user's unexpired episodes, newest first, so they can be reviewed by the user.
    /// </summary>
    Task<IReadOnlyList<RememberedEpisode>> ListAsync(
        string userId,
        int maxResults,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Appends a new episode for the user with an expiry based on the configured retention.
    /// </summary>
    Task AddAsync(
        string userId,
        string summary,
        IReadOnlyCollection<string> bookIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Deletes a single episode owned by the user.
    /// </summary>
    /// <returns><see langword="true" /> if an episode was deleted; otherwise <see langword="false" />.</returns>
    Task<bool> DeleteAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all episodes owned by the user.
    /// </summary>
    Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes every expired episode across all users (retention enforcement).
    /// </summary>
    /// <returns>The number of purged episodes.</returns>
    Task<int> PurgeExpiredAsync(CancellationToken cancellationToken = default);
}

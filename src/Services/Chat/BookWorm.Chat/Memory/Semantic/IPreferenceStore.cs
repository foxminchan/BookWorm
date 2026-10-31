namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     Durable, user-scoped storage for semantic memories (generalized preferences).
///     Every operation requires the owning user id so that data never crosses user boundaries.
/// </summary>
internal interface IPreferenceStore
{
    /// <summary>
    ///     Gets the user's strongest preferences, ranked by decayed confidence and recency,
    ///     excluding preferences whose confidence has decayed below the configured threshold.
    /// </summary>
    Task<IReadOnlyList<RememberedPreference>> GetRelevantAsync(
        string userId,
        int maxResults,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets every stored preference for the user so it can be reviewed by the user.
    /// </summary>
    Task<IReadOnlyList<RememberedPreference>> ListAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Consolidates newly observed candidates into the user's preferences
    ///     (reinforce on repeat, replace on contradiction) and prunes weak preferences.
    /// </summary>
    Task ApplyAsync(
        string userId,
        IReadOnlyCollection<PreferenceCandidate> candidates,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Deletes a single preference owned by the user.
    /// </summary>
    /// <returns><see langword="true" /> if a preference was deleted; otherwise <see langword="false" />.</returns>
    Task<bool> DeleteAsync(string userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all preferences owned by the user.
    /// </summary>
    Task DeleteAllAsync(string userId, CancellationToken cancellationToken = default);
}

namespace BookWorm.Chat.Memory;

/// <summary>
///     Facade over the episodic and semantic memory stores used by BookAgent and the memory endpoints.
/// </summary>
internal interface IUserMemoryService
{
    /// <summary>
    ///     Recalls the user's strongest preferences and the episodes most relevant to <paramref name="query" />.
    ///     Failures in either store are logged and degrade to an empty section instead of failing the chat.
    /// </summary>
    Task<UserMemorySnapshot> RecallAsync(
        string userId,
        string? query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Gets everything remembered about the user, for the "view my memory" endpoint.
    /// </summary>
    Task<UserMemorySnapshot> GetAllAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Forgets everything remembered about the user.
    /// </summary>
    Task ForgetAllAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Forgets a single preference owned by the user.
    /// </summary>
    Task<bool> ForgetPreferenceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    ///     Forgets a single episode owned by the user.
    /// </summary>
    Task<bool> ForgetEpisodeAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    );
}

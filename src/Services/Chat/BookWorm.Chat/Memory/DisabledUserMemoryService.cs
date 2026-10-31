namespace BookWorm.Chat.Memory;

/// <summary>
///     <see cref="IUserMemoryService" /> used when durable memory is disabled or its stores are not
///     configured: nothing is remembered, so there is nothing to recall or forget.
/// </summary>
internal sealed class DisabledUserMemoryService : IUserMemoryService
{
    public Task<UserMemorySnapshot> RecallAsync(
        string userId,
        string? query,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(UserMemorySnapshot.Empty);
    }

    public Task<UserMemorySnapshot> GetAllAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(UserMemorySnapshot.Empty);
    }

    public Task ForgetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ForgetPreferenceAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<bool> ForgetEpisodeAsync(
        string userId,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }
}

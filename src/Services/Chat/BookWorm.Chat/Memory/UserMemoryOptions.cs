using System.ComponentModel.DataAnnotations;

namespace BookWorm.Chat.Memory;

/// <summary>
///     Tuning and retention settings for BookAgent's cross-session user memory,
///     bound from the <c>Chat:Memory</c> configuration section.
/// </summary>
internal sealed class UserMemoryOptions
{
    /// <summary>
    ///     The configuration section that the options are bound from.
    /// </summary>
    public const string SectionName = "Chat:Memory";

    /// <summary>
    ///     Gets or sets a value indicating whether durable user memory is enabled.
    ///     Memory is also disabled automatically when its backing stores are not configured.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets the maximum number of semantic preferences injected per turn.
    /// </summary>
    [Range(1, 50)]
    public int MaxPreferences { get; set; } = 10;

    /// <summary>
    ///     Gets or sets the maximum number of episodic memories injected per turn.
    /// </summary>
    [Range(1, 20)]
    public int MaxEpisodes { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the maximum number of semantic preferences retained per user.
    ///     The weakest preferences are pruned once the limit is exceeded.
    /// </summary>
    [Range(1, 500)]
    public int MaxPreferencesPerUser { get; set; } = 50;

    /// <summary>
    ///     Gets or sets how long an episodic memory is retained before it expires.
    /// </summary>
    public TimeSpan EpisodeRetention { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    ///     Gets or sets the half-life used to decay the confidence of preferences
    ///     that have not been reinforced recently.
    /// </summary>
    public TimeSpan PreferenceHalfLife { get; set; } = TimeSpan.FromDays(180);

    /// <summary>
    ///     Gets or sets the minimum decayed confidence for a preference to be recalled or retained.
    /// </summary>
    [Range(0d, 1d)]
    public double MinPreferenceConfidence { get; set; } = 0.2;

    /// <summary>
    ///     Gets or sets the minimum similarity score for an episodic memory to be considered relevant.
    /// </summary>
    [Range(0d, 1d)]
    public double MinEpisodeRelevance { get; set; } = 0.3;

    /// <summary>
    ///     Gets or sets the capacity of the in-process extraction queue. When full, the oldest
    ///     pending exchange is dropped so that extraction never blocks the chat stream.
    /// </summary>
    [Range(1, 10_000)]
    public int ExtractionQueueCapacity { get; set; } = 256;

    /// <summary>
    ///     Gets or sets how often expired episodic memories are purged.
    /// </summary>
    public TimeSpan RetentionSweepInterval { get; set; } = TimeSpan.FromHours(6);
}

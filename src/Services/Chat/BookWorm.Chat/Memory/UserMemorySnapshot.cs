using BookWorm.Chat.Memory.Semantic;

namespace BookWorm.Chat.Memory;

/// <summary>
///     A semantic preference recalled for the current user, with its decayed confidence.
/// </summary>
internal sealed record RememberedPreference(
    Guid Id,
    PreferenceKind Kind,
    string Value,
    PreferenceSentiment Sentiment,
    double Confidence,
    int EvidenceCount,
    DateTime LastSeenAt
);

/// <summary>
///     An episodic memory (summary of a specific past interaction) recalled for the current user.
/// </summary>
internal sealed record RememberedEpisode(
    Guid Id,
    string Summary,
    IReadOnlyList<string> BookIds,
    DateTimeOffset CreatedAt,
    double? Relevance
);

/// <summary>
///     The memories recalled for a single user, kept as distinct semantic and episodic sections.
/// </summary>
internal sealed record UserMemorySnapshot(
    IReadOnlyList<RememberedPreference> Preferences,
    IReadOnlyList<RememberedEpisode> Episodes
)
{
    /// <summary>A snapshot that contains no memories.</summary>
    public static UserMemorySnapshot Empty { get; } = new([], []);

    /// <summary>Gets a value indicating whether no memories were recalled.</summary>
    public bool IsEmpty => Preferences.Count == 0 && Episodes.Count == 0;
}

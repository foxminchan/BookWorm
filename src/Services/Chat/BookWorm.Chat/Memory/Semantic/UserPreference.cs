namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     A generalized, user-scoped preference (semantic memory) persisted in the <c>user_preferences</c> table.
/// </summary>
/// <remarks>
///     Semantic memories are consolidated from many exchanges: repeated evidence reinforces a preference,
///     a contradiction replaces it, and confidence decays over time when it is not reinforced.
/// </remarks>
internal sealed class UserPreference
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public required string UserId { get; set; }

    public PreferenceKind Kind { get; set; }

    public required string Value { get; set; }

    public required string NormalizedValue { get; set; }

    public PreferenceSentiment Sentiment { get; set; }

    public double Confidence { get; set; }

    public int EvidenceCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastSeenAt { get; set; }
}

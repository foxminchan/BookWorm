using BookWorm.Chat.Memory.Semantic;

namespace BookWorm.Chat.Memory.Extraction;

/// <summary>
///     The memories extracted from a single exchange: at most one episode plus any explicitly stated preferences.
/// </summary>
/// <param name="EpisodeSummary">A short, anonymized summary of the exchange, or <see langword="null" /> if not memorable.</param>
/// <param name="BookIds">Catalog book identifiers referenced in the exchange.</param>
/// <param name="Preferences">Preferences the user explicitly stated.</param>
internal sealed record MemoryExtractionResult(
    string? EpisodeSummary,
    IReadOnlyList<string> BookIds,
    IReadOnlyList<PreferenceCandidate> Preferences
)
{
    /// <summary>A result containing nothing worth remembering.</summary>
    public static MemoryExtractionResult Empty { get; } = new(null, [], []);
}

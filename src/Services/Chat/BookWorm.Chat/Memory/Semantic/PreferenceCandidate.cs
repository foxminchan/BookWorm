namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     A preference explicitly stated by the user in a single exchange, extracted for consolidation
///     into the user's long-term semantic memory.
/// </summary>
/// <param name="Kind">The preference category.</param>
/// <param name="Value">The preference value, for example a genre or author name.</param>
/// <param name="Sentiment">Whether the user likes or wants to avoid the value.</param>
internal sealed record PreferenceCandidate(
    PreferenceKind Kind,
    string Value,
    PreferenceSentiment Sentiment
);

using BookWorm.Chat.Memory;

namespace BookWorm.Chat.Features.Memories;

/// <summary>
///     Everything BookAgent remembers about the current user.
/// </summary>
/// <param name="Preferences">Generalized preferences (semantic memory).</param>
/// <param name="Episodes">Summaries of specific past interactions (episodic memory).</param>
public sealed record UserMemoryDto(
    IReadOnlyList<PreferenceDto> Preferences,
    IReadOnlyList<EpisodeDto> Episodes
);

/// <summary>
///     A remembered preference.
/// </summary>
/// <param name="Id">The preference identifier, used to delete it.</param>
/// <param name="Kind">The category, for example <c>Genre</c> or <c>Author</c>.</param>
/// <param name="Value">The preference value.</param>
/// <param name="Sentiment"><c>Like</c> or <c>Dislike</c>.</param>
/// <param name="Confidence">The decayed confidence in the range [0, 1].</param>
/// <param name="EvidenceCount">How many times the preference has been stated.</param>
/// <param name="LastSeenAt">When the preference was last stated.</param>
public sealed record PreferenceDto(
    Guid Id,
    string Kind,
    string Value,
    string Sentiment,
    double Confidence,
    int EvidenceCount,
    DateTime LastSeenAt
);

/// <summary>
///     A remembered past interaction.
/// </summary>
/// <param name="Id">The episode identifier, used to delete it.</param>
/// <param name="Summary">An anonymized summary of the interaction.</param>
/// <param name="BookIds">Catalog books referenced in the interaction.</param>
/// <param name="CreatedAt">When the interaction happened.</param>
public sealed record EpisodeDto(
    Guid Id,
    string Summary,
    IReadOnlyList<string> BookIds,
    DateTimeOffset CreatedAt
);

internal static class UserMemoryDtoMapper
{
    public static UserMemoryDto ToDto(this UserMemorySnapshot snapshot)
    {
        return new(
            [
                .. snapshot.Preferences.Select(p => new PreferenceDto(
                    p.Id,
                    p.Kind.ToString(),
                    p.Value,
                    p.Sentiment.ToString(),
                    Math.Round(p.Confidence, 2),
                    p.EvidenceCount,
                    p.LastSeenAt
                )),
            ],
            [
                .. snapshot.Episodes.Select(e => new EpisodeDto(
                    e.Id,
                    e.Summary,
                    e.BookIds,
                    e.CreatedAt
                )),
            ]
        );
    }
}

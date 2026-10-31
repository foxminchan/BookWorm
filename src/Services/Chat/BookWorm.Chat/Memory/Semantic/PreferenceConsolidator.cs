namespace BookWorm.Chat.Memory.Semantic;

/// <summary>
///     Pure consolidation rules that turn per-exchange <see cref="PreferenceCandidate" /> evidence
///     into generalized <see cref="UserPreference" /> records.
/// </summary>
internal static class PreferenceConsolidator
{
    /// <summary>The confidence assigned to a newly observed (or freshly replaced) preference.</summary>
    public const double InitialConfidence = 0.5;

    /// <summary>The confidence added each time the same preference is stated again.</summary>
    public const double ReinforcementStep = 0.15;

    /// <summary>The maximum length of a stored preference value.</summary>
    public const int MaxValueLength = 100;

    /// <summary>
    ///     Normalizes a preference value so that equivalent mentions (casing, extra whitespace)
    ///     map to the same stored record.
    /// </summary>
    /// <param name="value">The raw preference value.</param>
    /// <returns>The normalized value used for de-duplication.</returns>
    public static string Normalize(string value)
    {
        return string.Join(
                ' ',
                value.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
            )
            .ToLowerInvariant();
    }

    /// <summary>
    ///     Merges a candidate into the user's existing preference for the same kind and value.
    /// </summary>
    /// <param name="existing">The stored preference, or <see langword="null" /> if none exists.</param>
    /// <param name="userId">The owner of the preference.</param>
    /// <param name="candidate">The newly observed preference.</param>
    /// <param name="now">The current UTC time.</param>
    /// <param name="halfLife">The confidence decay half-life.</param>
    /// <returns>
    ///     The created or updated preference. When <paramref name="existing" /> is provided it is mutated in place.
    /// </returns>
    /// <remarks>
    ///     Repeat evidence reinforces the (decayed) confidence; contradicting evidence (opposite sentiment)
    ///     replaces the stored sentiment and resets confidence, so user corrections win immediately.
    /// </remarks>
    public static UserPreference Merge(
        UserPreference? existing,
        string userId,
        PreferenceCandidate candidate,
        DateTime now,
        TimeSpan halfLife
    )
    {
        var value = candidate.Value.Trim();

        if (existing is null)
        {
            return new()
            {
                UserId = userId,
                Kind = candidate.Kind,
                Value = value,
                NormalizedValue = Normalize(value),
                Sentiment = candidate.Sentiment,
                Confidence = InitialConfidence,
                EvidenceCount = 1,
                CreatedAt = now,
                LastSeenAt = now,
            };
        }

        if (existing.Sentiment == candidate.Sentiment)
        {
            existing.Confidence = Math.Min(
                1d,
                GetEffectiveConfidence(existing, now, halfLife) + ReinforcementStep
            );
            existing.EvidenceCount++;
        }
        else
        {
            existing.Sentiment = candidate.Sentiment;
            existing.Confidence = InitialConfidence;
            existing.EvidenceCount = 1;
        }

        existing.Value = value;
        existing.LastSeenAt = now;

        return existing;
    }

    /// <summary>
    ///     Computes the confidence of a preference after exponential decay since it was last reinforced.
    /// </summary>
    /// <param name="preference">The stored preference.</param>
    /// <param name="now">The current UTC time.</param>
    /// <param name="halfLife">The decay half-life.</param>
    /// <returns>The decayed confidence in the range [0, 1].</returns>
    public static double GetEffectiveConfidence(
        UserPreference preference,
        DateTime now,
        TimeSpan halfLife
    )
    {
        var age = now - preference.LastSeenAt;

        if (age <= TimeSpan.Zero || halfLife <= TimeSpan.Zero)
        {
            return preference.Confidence;
        }

        return preference.Confidence * Math.Pow(0.5, age / halfLife);
    }
}

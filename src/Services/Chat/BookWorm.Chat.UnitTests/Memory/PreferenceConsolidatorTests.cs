using BookWorm.Chat.Memory.Semantic;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class PreferenceConsolidatorTests
{
    private const string UserId = "user-a";
    private static readonly DateTime _now = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly TimeSpan _halfLife = TimeSpan.FromDays(180);

    [Test]
    public void GivenNoExistingPreference_WhenMerging_ThenShouldCreatePreferenceWithInitialConfidence()
    {
        var candidate = new PreferenceCandidate(
            PreferenceKind.Genre,
            "  Science   Fiction ",
            PreferenceSentiment.Like
        );

        var result = PreferenceConsolidator.Merge(null, UserId, candidate, _now, _halfLife);

        result.UserId.ShouldBe(UserId);
        result.Kind.ShouldBe(PreferenceKind.Genre);
        result.Value.ShouldBe("Science   Fiction");
        result.NormalizedValue.ShouldBe("science fiction");
        result.Sentiment.ShouldBe(PreferenceSentiment.Like);
        result.Confidence.ShouldBe(PreferenceConsolidator.InitialConfidence);
        result.EvidenceCount.ShouldBe(1);
        result.CreatedAt.ShouldBe(_now);
        result.LastSeenAt.ShouldBe(_now);
    }

    [Test]
    public void GivenRepeatedEvidence_WhenMerging_ThenShouldReinforcePreference()
    {
        var existing = CreatePreference(PreferenceSentiment.Like, 0.5, _now);
        var candidate = new PreferenceCandidate(
            PreferenceKind.Genre,
            "Science Fiction",
            PreferenceSentiment.Like
        );

        var result = PreferenceConsolidator.Merge(existing, UserId, candidate, _now, _halfLife);

        result.ShouldBeSameAs(existing);
        result.Confidence.ShouldBe(0.5 + PreferenceConsolidator.ReinforcementStep, 1e-9);
        result.EvidenceCount.ShouldBe(2);
        result.Sentiment.ShouldBe(PreferenceSentiment.Like);
    }

    [Test]
    public void GivenStronglyHeldPreference_WhenReinforcing_ThenConfidenceShouldNotExceedOne()
    {
        var existing = CreatePreference(PreferenceSentiment.Like, 0.95, _now);
        var candidate = new PreferenceCandidate(
            PreferenceKind.Genre,
            "science fiction",
            PreferenceSentiment.Like
        );

        var result = PreferenceConsolidator.Merge(existing, UserId, candidate, _now, _halfLife);

        result.Confidence.ShouldBe(1d);
    }

    [Test]
    public void GivenContradictingEvidence_WhenMerging_ThenShouldReplaceSentimentAndResetConfidence()
    {
        var existing = CreatePreference(PreferenceSentiment.Like, 0.9, _now.AddDays(-1));
        existing.EvidenceCount = 5;
        var candidate = new PreferenceCandidate(
            PreferenceKind.Genre,
            "Science Fiction",
            PreferenceSentiment.Dislike
        );

        var result = PreferenceConsolidator.Merge(existing, UserId, candidate, _now, _halfLife);

        result.Sentiment.ShouldBe(PreferenceSentiment.Dislike);
        result.Confidence.ShouldBe(PreferenceConsolidator.InitialConfidence);
        result.EvidenceCount.ShouldBe(1);
        result.LastSeenAt.ShouldBe(_now);
    }

    [Test]
    public void GivenStalePreference_WhenComputingEffectiveConfidence_ThenShouldDecayByHalfLife()
    {
        var preference = CreatePreference(PreferenceSentiment.Like, 0.8, _now - _halfLife);

        var confidence = PreferenceConsolidator.GetEffectiveConfidence(preference, _now, _halfLife);

        confidence.ShouldBe(0.4, 1e-9);
    }

    [Test]
    public void GivenStalePreference_WhenReinforcing_ThenShouldReinforceFromDecayedConfidence()
    {
        var existing = CreatePreference(PreferenceSentiment.Like, 0.8, _now - _halfLife);
        var candidate = new PreferenceCandidate(
            PreferenceKind.Genre,
            "science fiction",
            PreferenceSentiment.Like
        );

        var result = PreferenceConsolidator.Merge(existing, UserId, candidate, _now, _halfLife);

        result.Confidence.ShouldBe(0.4 + PreferenceConsolidator.ReinforcementStep, 1e-9);
    }

    private static UserPreference CreatePreference(
        PreferenceSentiment sentiment,
        double confidence,
        DateTime lastSeenAt
    )
    {
        return new()
        {
            UserId = UserId,
            Kind = PreferenceKind.Genre,
            Value = "Science Fiction",
            NormalizedValue = "science fiction",
            Sentiment = sentiment,
            Confidence = confidence,
            EvidenceCount = 1,
            CreatedAt = lastSeenAt,
            LastSeenAt = lastSeenAt,
        };
    }
}

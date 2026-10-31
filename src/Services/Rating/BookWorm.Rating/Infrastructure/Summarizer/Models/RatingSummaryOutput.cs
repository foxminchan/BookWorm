namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

internal sealed record RatingSummaryOutput(
    string OverallSentiment,
    string RecurringThemes,
    string ReportedStrengths,
    string ReportedWeaknesses,
    string ReviewBasedQualitySignals,
    string DataLimitations
)
{
    public string ToText()
    {
        return $"""
            Overall sentiment: {OverallSentiment}
            Recurring themes: {RecurringThemes}
            Reported strengths: {ReportedStrengths}
            Reported weaknesses: {ReportedWeaknesses}
            Review-based quality signals: {ReviewBasedQualitySignals}
            Data limitations: {DataLimitations}
            """;
    }
}

namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

internal sealed record QualityAssessmentOutput
{
    public bool DominantNegativePattern { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string Confidence { get; init; } = string.Empty;
}

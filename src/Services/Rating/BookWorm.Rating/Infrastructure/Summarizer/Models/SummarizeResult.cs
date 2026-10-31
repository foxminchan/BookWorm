namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

public sealed record SummarizeResult(
    string Summary,
    RatingClassification Classification = RatingClassification.NoData,
    double? AverageRating = null,
    int ReviewCount = 0,
    string Evidence = "",
    string Confidence = ""
);

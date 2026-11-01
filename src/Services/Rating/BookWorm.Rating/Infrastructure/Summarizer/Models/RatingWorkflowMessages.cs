namespace BookWorm.Rating.Infrastructure.Summarizer.Models;

internal sealed record RatingWorkflowInput(
    FeedbackReview[] Reviews,
    double AverageRating,
    int ReviewCount
);

internal sealed record RatingWorkflowMetrics(double AverageRating, int ReviewCount);

internal sealed record ReviewBatch(int Index, FeedbackReview[] Reviews);

internal sealed record ReviewPartition(
    ReviewBatch[][] WorkerBatches,
    RatingWorkflowMetrics Metrics
);

internal sealed record PartialRatingSummary(
    int BatchIndex,
    int ReviewCount,
    RatingSummaryOutput Summary
);

internal sealed class MapShardResult(
    int workerIndex,
    PartialRatingSummary[] summaries,
    RatingWorkflowMetrics metrics
) : WorkflowEvent
{
    public int WorkerIndex { get; } = workerIndex;

    public PartialRatingSummary[] Summaries { get; } = summaries;

    public RatingWorkflowMetrics Metrics { get; } = metrics;
}

internal sealed record ReductionRequest(
    PartialRatingSummary[] Summaries,
    RatingWorkflowMetrics Metrics,
    int Iteration,
    string? AssessmentFeedback
);

internal sealed record RatingSummaryCandidate(
    RatingSummaryOutput Summary,
    PartialRatingSummary[] PartialSummaries,
    RatingWorkflowMetrics Metrics,
    int Iteration
);

internal sealed record QualityAssessmentDecision(
    RatingSummaryCandidate Candidate,
    QualityAssessmentOutput Assessment
);

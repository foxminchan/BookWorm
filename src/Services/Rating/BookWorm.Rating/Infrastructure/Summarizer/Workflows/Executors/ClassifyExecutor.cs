namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

internal sealed class ClassifyExecutor()
    : Executor<QualityAssessmentDecision, SummarizeResult>(nameof(ClassifyExecutor))
{
    public override ValueTask<SummarizeResult> HandleAsync(
        QualityAssessmentDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var metrics = message.Candidate.Metrics;

        var classification = RatingClassifier.Classify(
            metrics.AverageRating,
            metrics.ReviewCount,
            message.Assessment.DominantNegativePattern
        );

        return ValueTask.FromResult(
            new SummarizeResult(
                message.Candidate.Summary.ToText(),
                classification,
                metrics.AverageRating,
                metrics.ReviewCount,
                message.Assessment.Evidence,
                message.Assessment.Confidence
            )
        );
    }
}

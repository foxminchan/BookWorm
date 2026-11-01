using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows.Runners;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

internal sealed class QualityAssessExecutor(
    IRatingPromptProvider promptProvider,
    StructuredRatingResponseRunner structuredResponseRunner
) : Executor<RatingSummaryCandidate, QualityAssessmentDecision>(nameof(QualityAssessExecutor))
{
    public override async ValueTask<QualityAssessmentDecision> HandleAsync(
        RatingSummaryCandidate message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var prompt = await promptProvider.GetQualityPromptAsync(
            message.Metrics.AverageRating,
            message.Metrics.ReviewCount,
            message.Summary.ToText(),
            cancellationToken
        );

        var assessment = await structuredResponseRunner.RunAsync(
            RatingSummarizerSerializationContext.Default.QualityAssessmentOutput,
            prompt,
            cancellationToken
        );

        return new(message, assessment);
    }
}

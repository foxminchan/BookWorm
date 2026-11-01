using System.Text.Json;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows.Runners;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

internal sealed class ReduceExecutor(
    IRatingPromptProvider promptProvider,
    StructuredRatingResponseRunner structuredResponseRunner
) : Executor<ReductionRequest, RatingSummaryCandidate>(nameof(ReduceExecutor))
{
    public override async ValueTask<RatingSummaryCandidate> HandleAsync(
        ReductionRequest message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        RatingSummaryOutput summary;

        if (message is { Iteration: 0, Summaries.Length: 1 })
        {
            summary = message.Summaries[0].Summary;
        }
        else
        {
            var promptData = JsonSerializer.Serialize(
                message.Summaries,
                RatingSummarizerSerializationContext.Default.PartialRatingSummaryArray
            );

            var prompt = await promptProvider.GetReductionPromptAsync(
                promptData,
                message.AssessmentFeedback,
                cancellationToken
            );

            summary = await structuredResponseRunner.RunAsync(
                RatingSummarizerSerializationContext.Default.RatingSummaryOutput,
                prompt,
                cancellationToken
            );
        }

        return new(summary, message.Summaries, message.Metrics, message.Iteration);
    }
}

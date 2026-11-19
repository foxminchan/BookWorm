using System.Text.Json;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows.Runners;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

[SendsMessage(typeof(MapShardResult))]
internal sealed class MapExecutor(
    int workerIndex,
    IRatingPromptProvider promptProvider,
    StructuredRatingResponseRunner structuredResponseRunner
) : Executor<ReviewPartition>($"{nameof(MapExecutor)}-{workerIndex}")
{
    public override async ValueTask HandleAsync(
        ReviewPartition message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        if (workerIndex < 0 || workerIndex >= message.WorkerBatches.Length)
        {
            throw new InvalidOperationException(
                "The workflow partition did not include this map worker."
            );
        }

        List<PartialRatingSummary> summaries = [with(message.WorkerBatches[workerIndex].Length)];

        foreach (var batch in message.WorkerBatches[workerIndex])
        {
            var reviewData = JsonSerializer.Serialize(
                batch.Reviews,
                RatingSummarizerSerializationContext.Default.FeedbackReviewArray
            );
            var prompt = await promptProvider.GetSummaryPromptAsync(reviewData, cancellationToken);
            var summary = await structuredResponseRunner.RunAsync(
                RatingSummarizerSerializationContext.Default.RatingSummaryOutput,
                prompt,
                cancellationToken
            );

            summaries.Add(new(batch.Index, batch.Reviews.Length, summary));
        }

        await context.SendMessageAsync(
            new MapShardResult(workerIndex, [.. summaries], message.Metrics),
            cancellationToken
        );
    }
}

using BookWorm.Constants.Other;
using BookWorm.Rating.Infrastructure.Summarizer.Providers;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows.Runners;
using Microsoft.Agents.AI;
using MAFWorkflow = Microsoft.Agents.AI.Workflows.Workflow;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows;

internal interface IRatingSummarizerWorkflowFactory
{
    MAFWorkflow Create(int reviewCount);
}

internal sealed class RatingSummarizerWorkflowFactory(
    [FromKeyedServices(Agents.RatingAgent)] AIAgent ratingAgent,
    IRatingPromptProvider promptProvider,
    RatingSummarizerOptions options
) : IRatingSummarizerWorkflowFactory
{
    public MAFWorkflow Create(int reviewCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reviewCount);

        var batchCount = ((reviewCount - 1) / options.BatchSize) + 1;
        var workerCount = Math.Min(batchCount, options.MaxConcurrency);
        var structuredResponseRunner = new StructuredRatingResponseRunner(ratingAgent);
        var partition = new PartitionExecutor(options.BatchSize, options.MaxConcurrency);
        var maps = Enumerable
            .Range(0, workerCount)
            .Select(index => new MapExecutor(index, promptProvider, structuredResponseRunner))
            .ToArray();
        var join = new MapResultsJoinExecutor(workerCount);
        var reduce = new ReduceExecutor(promptProvider, structuredResponseRunner);
        var quality = new QualityAssessExecutor(promptProvider, structuredResponseRunner);
        var refine = new RefineReductionExecutor();
        var classify = new ClassifyExecutor();

        // Build per invocation so no workflow captures the scoped feedback repository.
        return new WorkflowBuilder(partition)
            .AddFanOutEdge(partition, [.. maps])
            .AddFanInBarrierEdge([.. maps], join)
            .AddEdge(join, reduce)
            .AddEdge(reduce, quality)
            .AddSwitch(quality, switchBuilder =>
                switchBuilder
                    .AddCase<QualityAssessmentDecision>(
                        decision =>
                            decision is not null
                            && decision.Candidate.Iteration < options.MaxIterations
                            && RatingConfidence.Parse(decision.Assessment.Confidence)
                                < options.MinimumConfidence,
                        refine
                    )
                    .WithDefault(classify)
            )
            .AddEdge(refine, reduce)
            .WithOutputFrom(classify)
            .Build();
    }
}

internal static class RatingConfidence
{
    public static RatingConfidenceLevel Parse(string confidence)
    {
        return
            Enum.TryParse<RatingConfidenceLevel>(confidence, true, out var level)
            && Enum.IsDefined(level)
            ? level
            : RatingConfidenceLevel.Unknown;
    }
}

using BookWorm.Rating.Infrastructure.Summarizer.Models;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer.Workflows.Executors;

internal sealed class RefineReductionExecutor()
    : Executor<QualityAssessmentDecision, ReductionRequest>(nameof(RefineReductionExecutor))
{
    public override ValueTask<ReductionRequest> HandleAsync(
        QualityAssessmentDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var feedback = $"""
            Previously assessed merged summary:
            {message.Candidate.Summary.ToText()}

            Previous confidence: {message.Assessment.Confidence}.
            Assessment evidence: {message.Assessment.Evidence}
            Revise the merged summary to address this evidence while remaining grounded in the partial summaries.
            """;

        return ValueTask.FromResult(
            new ReductionRequest(
                message.Candidate.PartialSummaries,
                message.Candidate.Metrics,
                message.Candidate.Iteration + 1,
                feedback
            )
        );
    }
}

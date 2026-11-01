using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using BookWorm.Rating.Infrastructure.Summarizer.Workflows;
using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Rating.Infrastructure.Summarizer;

internal sealed class RatingSummarizer(
    IFeedbackRepository feedbackRepository,
    IRatingSummarizerWorkflowFactory workflowFactory
) : ISummarizer
{
    public async Task<SummarizeResult> SummarizeAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    )
    {
        var feedback = await feedbackRepository.ListAsync(
            new FeedbackFilterSpec(bookId, nameof(Feedback.Rating), false, 1, int.MaxValue),
            cancellationToken
        );
        var reviewCount = feedback.Count;

        if (reviewCount == 0)
        {
            return new("No feedback found for this book.");
        }

        var averageRating = feedback.Average(review => review.Rating);
        var reviews = feedback
            .Select(review => new FeedbackReview(
                (Guid)review.Id,
                review.FirstName,
                review.LastName,
                review.Comment,
                review.Rating,
                review.BookId
            ))
            .ToArray();

        var workflow = workflowFactory.Create(reviewCount);

        await using var run = await InProcessExecution.RunAsync(
            workflow,
            new RatingWorkflowInput(reviews, averageRating, reviewCount),
            cancellationToken: cancellationToken
        );

        var result = run
            .OutgoingEvents.OfType<WorkflowOutputEvent>()
            .Select(output => output.Data)
            .OfType<SummarizeResult>()
            .SingleOrDefault();

        if (result is not null)
        {
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var workflowError = run.OutgoingEvents.OfType<WorkflowErrorEvent>().FirstOrDefault();
        throw new InvalidOperationException(
            $"The rating summarization workflow completed without producing a result. "
                + $"Workflow events: {string.Join(", ", run.OutgoingEvents.Select(evt => evt.GetType().Name))}.",
            workflowError?.Exception
        );
    }
}

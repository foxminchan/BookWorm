using BookWorm.Rating.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Features.Summarize;

public sealed record SummarizeFeedbackQuery(Guid BookId) : IQuery<SummarizeResult>;

public sealed class SummarizeFeedbackHandler(
    [FromKeyedServices(nameof(RatingAgent))] ChatCompletionAgent agent
) : IQueryHandler<SummarizeFeedbackQuery, SummarizeResult>
{
    /// <summary>
    /// Processes a request to generate a summary of ratings for a specified book.
    /// </summary>
    /// <param name="request">The query containing the book ID to summarize ratings for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A summary of the book's ratings.</returns>
    /// <exception cref="NotFoundException">Thrown if no ratings are found for the specified book.</exception>
    public async Task<SummarizeResult> Handle(
        SummarizeFeedbackQuery request,
        CancellationToken cancellationToken
    )
    {
        ChatHistoryAgentThread agentThread = new();

        var result = await agent
            .InvokeAsync(
                $"Get an overview of the ratings for book with ID {request.BookId}",
                agentThread,
                cancellationToken: cancellationToken
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Message.Content))
        {
            throw new NotFoundException($"No ratings found for book with ID {request.BookId}");
        }

        return result.Message.Content;
    }
}

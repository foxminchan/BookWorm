using BookWorm.Rating.Agents;
using Microsoft.SemanticKernel.Agents;

namespace BookWorm.Rating.Features.Summarize;

public sealed record SummarizeFeedbackQuery(Guid BookId) : IQuery<SummarizeResult>;

public sealed class SummarizeFeedbackHandler(
    [FromKeyedServices(nameof(RatingAgent))] ChatCompletionAgent agent
) : IQueryHandler<SummarizeFeedbackQuery, SummarizeResult>
{
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

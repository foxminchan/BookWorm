using BookWorm.Chassis.CQRS.Query;
using BookWorm.Rating.Infrastructure.Summarizer;

namespace BookWorm.Rating.Features.Summarize;

public sealed record SummarizeFeedbackQuery(Guid BookId) : IQuery<SummarizeResult>;

public sealed class SummarizeFeedbackHandler(ISummarizer summarizer)
    : IQueryHandler<SummarizeFeedbackQuery, SummarizeResult>
{
    public async Task<SummarizeResult> Handle(
        SummarizeFeedbackQuery request,
        CancellationToken cancellationToken
    )
    {
        var result =
            await summarizer.SummarizeAsync(
                $"Get an overview of the ratings for book with ID  {request.BookId}",
                cancellationToken
            ) ?? throw new NotFoundException($"No ratings found for book with ID {request.BookId}");

        return result;
    }
}

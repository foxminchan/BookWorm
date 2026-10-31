using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.Rating.Infrastructure.Summarizer.Models;
using Mediator;

namespace BookWorm.Rating.Features.Summarize;

public sealed record SummarizeFeedbackQuery(Guid BookId) : IQuery<SummarizeResult>;

internal sealed class SummarizeFeedbackHandler(ISummarizer summarizer)
    : IQueryHandler<SummarizeFeedbackQuery, SummarizeResult>
{
    public async ValueTask<SummarizeResult> Handle(
        SummarizeFeedbackQuery request,
        CancellationToken cancellationToken
    )
    {
        return await summarizer.SummarizeAsync(request.BookId, cancellationToken);
    }
}

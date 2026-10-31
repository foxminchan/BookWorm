using BookWorm.Rating.Infrastructure.Summarizer.Models;

namespace BookWorm.Rating.Infrastructure.Summarizer;

public interface ISummarizer
{
    Task<SummarizeResult> SummarizeAsync(
        Guid bookId,
        CancellationToken cancellationToken = default
    );
}

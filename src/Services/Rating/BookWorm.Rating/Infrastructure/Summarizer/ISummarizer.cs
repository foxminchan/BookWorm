namespace BookWorm.Rating.Infrastructure.Summarizer;

public interface ISummarizer
{
    Task<string?> SummarizeAsync(string content, CancellationToken cancellationToken = default);
}

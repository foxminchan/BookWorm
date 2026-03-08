namespace BookWorm.Rating.Infrastructure.Summarizer;

internal interface ISummarizer
{
    Task<string?> SummarizeAsync(string content, CancellationToken cancellationToken = default);
}

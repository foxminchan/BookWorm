namespace BookWorm.Rating.Infrastructure.Summarizer.Providers;

internal interface IRatingPromptProvider
{
    Task<ChatMessage> GetSummaryPromptAsync(string reviewData, CancellationToken cancellationToken);

    Task<ChatMessage> GetReductionPromptAsync(
        string partialSummaries,
        string? assessmentFeedback,
        CancellationToken cancellationToken
    );

    Task<ChatMessage> GetQualityPromptAsync(
        double averageRating,
        int reviewCount,
        string reviewSummary,
        CancellationToken cancellationToken
    );
}

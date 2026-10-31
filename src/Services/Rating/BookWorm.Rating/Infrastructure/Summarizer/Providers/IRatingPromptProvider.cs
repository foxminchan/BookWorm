using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Infrastructure.Summarizer.Providers;

internal interface IRatingPromptProvider
{
    Task<ChatMessage> GetSummaryPromptAsync(string reviewData, CancellationToken cancellationToken);

    Task<ChatMessage> GetQualityPromptAsync(
        double averageRating,
        int reviewCount,
        string reviewSummary,
        CancellationToken cancellationToken
    );
}

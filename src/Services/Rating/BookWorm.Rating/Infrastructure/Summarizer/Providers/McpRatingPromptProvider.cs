using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.Summarizer.Providers;

internal sealed class McpRatingPromptProvider(McpClient mcpClient) : IRatingPromptProvider
{
    public async Task<ChatMessage> GetSummaryPromptAsync(
        string reviewData,
        CancellationToken cancellationToken
    )
    {
        var prompt = await mcpClient.GetPromptAsync(
            "summarize_rating",
            new Dictionary<string, object?> { ["content"] = reviewData },
            cancellationToken: cancellationToken
        );

        return prompt.ToChatMessages().Single();
    }

    public async Task<ChatMessage> GetQualityPromptAsync(
        double averageRating,
        int reviewCount,
        string reviewSummary,
        CancellationToken cancellationToken
    )
    {
        var prompt = await mcpClient.GetPromptAsync(
            "analyze_book_quality",
            new Dictionary<string, object?>
            {
                ["averageRating"] = averageRating,
                ["totalReviews"] = reviewCount,
                ["reviewSummary"] = reviewSummary,
            },
            cancellationToken: cancellationToken
        );

        return prompt.ToChatMessages().Single();
    }
}

using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Prompts;

[McpServerPromptType]
public sealed class BookQualityPromptType
{
    [McpMeta("category", "quality_analysis")]
    [McpServerPrompt(Name = "analyze_book_quality", Title = "Analyze Book Quality")]
    [Description(
        "Classifies a book's market quality as Best Seller, Good, Bad, or No Data based on rating metrics"
    )]
    [return: Description("A structured classification prompt for book quality analysis")]
    public static ChatMessage AnalyzeBookQualityPrompt(
        [Description("The book's average rating score (0.0-5.0)")] double averageRating,
        [Description("Total number of reviews submitted for the book")] int totalReviews,
        [Description("A text summary of the reviews (from the summarize_rating prompt)")]
            string reviewSummary
    )
    {
        return new(
            ChatRole.User,
            $"""
                Classify the quality and market position of this book based on its rating data:

                Rating Data:
                - Average rating: {averageRating:F1} / 5.0
                - Total reviews: {totalReviews}
                - Review summary: {reviewSummary}

                Classification Rules:
                - Best Seller: avg >= 4.5 AND reviews >= 50, OR avg >= 4.0 AND reviews >= 200
                - Good: avg >= 3.5 AND reviews >= 10
                - Bad: avg < 3.5 OR dominant negative sentiment patterns
                - No Data: fewer than 3 reviews or no rating available

                Provide:
                1. Classification (Best Seller / Good / Bad / No Data)
                2. Key supporting metrics
                3. Brief justification (2-3 sentences)
                4. Confidence level (High / Medium / Low) with reason
            """
        );
    }
}

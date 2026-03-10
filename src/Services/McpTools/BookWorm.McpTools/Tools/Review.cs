using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Review(IRatingApi ratingApi)
{
    [McpMeta("category", "reviews")]
    [McpServerTool(Name = "get_book_reviews", Title = "Get Book Reviews")]
    [Description("Retrieves customer reviews and ratings for a specific book")]
    [return: Description("A JSON array of reviews for the book or a not found message")]
    public async Task<string> GetBookReviewsAsync(
        [Description("The unique identifier of the book to get reviews for")] Guid bookId
    )
    {
        var response = await ratingApi.ListFeedbacksAsync(bookId);

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error retrieving reviews. Please try again later.";
        }

        return response.Content is null or { Count: 0 }
            ? $"No reviews found for book with ID {bookId}."
            : JsonSerializer.Serialize(
                response.Content,
                FeedbackSerializationContext.Default.ListFeedback
            );
    }
}

[JsonSerializable(typeof(List<Feedback>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;

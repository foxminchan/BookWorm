using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Tools;

[McpServerToolType]
public sealed class Rating(IRatingApi ratingApi)
{
    [McpMeta("category", "rating")]
    [McpServerTool(Name = "GetBookRating", Title = "Get Book Rating")]
    [Description("Retrieves the rating for a specified book by its ID")]
    [return: Description("The rating of the book or an error message")]
    public async Task<string> GetBookRatingAsync(
        [Description("The unique identifier of the book")] Guid bookId
    )
    {
        var response = await ratingApi.ListFeedbacksAsync(bookId);

        if (!response.IsSuccessStatusCode)
        {
            return "There was an error while retrieving the book rating. Please try again later.";
        }

        return response.Content?.Items.Count == 0
            ? "We couldn't find any ratings for this book."
            : JsonSerializer.Serialize(
                response.Content?.Items,
                FeedbackSerializationContext.Default.IReadOnlyListFeedback
            );
    }
}

[JsonSerializable(typeof(IReadOnlyList<Feedback>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;

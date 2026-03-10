using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.McpTools.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BookWorm.McpTools.Resources;

[McpServerResourceType]
public sealed class RatingResourceProvider(IRatingApi ratingApi)
{
    [McpServerResource(
        UriTemplate = "bookworm://ratings/{bookId}/reviews",
        Name = "Book Reviews",
        MimeType = MediaTypeNames.Application.Json
    )]
    [Description("Customer reviews and ratings for a specific book")]
    public async Task<string> GetBookReviewsAsync(
        [Description("The unique identifier of the book")] Guid bookId
    )
    {
        var response = await ratingApi.ListFeedbacksAsync(bookId);

        if (!response.IsSuccessStatusCode)
        {
            throw new McpException($"Reviews for book with ID {bookId} could not be retrieved.");
        }

        return response.Content is null or { Count: 0 }
            ? "[]"
            : JsonSerializer.Serialize(
                response.Content,
                RatingResourceSerializationContext.Default.ListFeedback
            );
    }
}

[JsonSerializable(typeof(List<Feedback>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class RatingResourceSerializationContext : JsonSerializerContext;

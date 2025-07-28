using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;
using Microsoft.SemanticKernel;

namespace BookWorm.Rating.Plugins;

public sealed class ReviewPlugin(ISender sender)
{
    [KernelFunction(nameof(GetCustomerReviews))]
    [Description(
        "Fetches all reviews and ratings for a book, returning review IDs, ratings, and comments in JSON"
    )]
    public async Task<string> GetCustomerReviews(
        [Description("The ID of the book to get the review for")] Guid bookId
    )
    {
        var reviews = await sender.Send(new ListFeedbacksQuery(bookId, 1, int.MaxValue));

        return reviews.Items.Any()
            ? JsonSerializer.Serialize(
                reviews.Items,
                FeedbackSerializationContext.Default.IReadOnlyListFeedbackDto
            )
            : "No reviews found for this book";
    }
}

[JsonSerializable(typeof(FeedbackDto))]
[JsonSerializable(typeof(IReadOnlyList<FeedbackDto>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;

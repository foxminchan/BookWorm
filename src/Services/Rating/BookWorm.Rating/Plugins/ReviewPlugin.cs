using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.Rating.Features;
using Microsoft.SemanticKernel;

namespace BookWorm.Rating.Plugins;

[ExcludeFromCodeCoverage]
public sealed class ReviewPlugin(IServiceScopeFactory factory)
{
    /// <summary>
    /// Retrieves all customer reviews and ratings for the specified book.
    /// </summary>
    /// <param name="bookId">The unique identifier of the book to retrieve reviews for.</param>
    /// <returns>
    /// A JSON string containing a list of review IDs, ratings, and comments if reviews exist; otherwise, a message indicating no reviews were found.
    /// </returns>
    [KernelFunction(nameof(GetCustomerReviews))]
    [Description(
        "Fetches all reviews and ratings for a book, returning review IDs, ratings, and comments in JSON"
    )]
    public async Task<string> GetCustomerReviews(
        [Description("The ID of the book to get the review for")] Guid bookId
    )
    {
        using var scope = factory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFeedbackRepository>();

        var reviews = await repository.ListAsync(new FeedbackFilterSpec(bookId, null, false));

        return reviews.Any()
            ? JsonSerializer.Serialize(
                reviews.ToFeedbackDtos(),
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

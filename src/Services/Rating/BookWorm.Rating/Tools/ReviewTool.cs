using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;

namespace BookWorm.Rating.Tools;

public sealed class ReviewTool
{
    private readonly Func<Guid, Task<string>> _getCustomerReviews;

    public ReviewTool(IServiceScopeFactory scopeFactory)
    {
        _getCustomerReviews = async bookId =>
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            return await GetCustomerReviewsAsync(sender, bookId);
        };
    }

    internal ReviewTool(ISender sender)
    {
        _getCustomerReviews = bookId => GetCustomerReviewsAsync(sender, bookId);
    }

    [Description(
        "Fetches all reviews and ratings for a book, returning review IDs, ratings, and comments in JSON"
    )]
    public Task<string> GetCustomerReviews(
        [Description("The ID of the book to get the review for")] Guid bookId
    )
    {
        return _getCustomerReviews(bookId);
    }

    private static async Task<string> GetCustomerReviewsAsync(ISender sender, Guid bookId)
    {
        var reviews = await sender.Send(new ListFeedbacksQuery(bookId, 1, int.MaxValue));

        return reviews.Any()
            ? JsonSerializer.Serialize(
                reviews,
                FeedbackSerializationContext.Default.IReadOnlyListFeedbackDto
            )
            : "No reviews found for this book";
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(GetCustomerReviews);
    }
}

[JsonSerializable(typeof(FeedbackDto))]
[JsonSerializable(typeof(IReadOnlyList<FeedbackDto>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;

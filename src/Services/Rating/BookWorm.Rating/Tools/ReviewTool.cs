using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.Create;
using BookWorm.Rating.Features.List;
using Mediator;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.Tools;

public sealed class ReviewTool(ISender sender)
{
    [Description(
        "Fetches all reviews and ratings for a book, returning review IDs, ratings, and comments in JSON"
    )]
    public async Task<string> GetCustomerReviews(
        [Description("The ID of the book to get the review for")] Guid bookId
    )
    {
        var reviews = await sender.Send(new ListFeedbacksQuery(bookId, 1, int.MaxValue));

        return reviews.Any()
            ? JsonSerializer.Serialize(
                reviews,
                FeedbackSerializationContext.Default.IReadOnlyListFeedbackDto
            )
            : "No reviews found for this book";
    }

    [Description(
        "Submit or replace a customer review for a book. This action REQUIRES explicit customer approval before execution."
    )]
    public async Task<string> SubmitReview(
        [Description("The UUID of the book to review")] Guid bookId,
        [Description("Star rating from 1 (lowest) to 5 (highest)")] int rating,
        [Description("Optional review comment (maximum 2000 characters)")] string? comment,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default
    )
    {
        if (rating is < 1 or > 5)
        {
            return "Rating must be between 1 and 5.";
        }

        if (comment?.Length > 2000)
        {
            return "Comment must not exceed 2000 characters.";
        }

        var firstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;

        await sender.Send(
            new CreateFeedbackCommand(bookId, firstName, lastName, comment, rating),
            cancellationToken
        );

        return $"Successfully submitted your {rating}-star review for book {bookId}.";
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(GetCustomerReviews);
        yield return new ApprovalRequiredAIFunction(
            AIFunctionFactory.Create(
                SubmitReview,
                "submitReview",
                "Submit a book review with rating and optional comment. REQUIRES explicit customer approval."
            )
        );
    }
}

[JsonSerializable(typeof(FeedbackDto))]
[JsonSerializable(typeof(IReadOnlyList<FeedbackDto>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class FeedbackSerializationContext : JsonSerializerContext;

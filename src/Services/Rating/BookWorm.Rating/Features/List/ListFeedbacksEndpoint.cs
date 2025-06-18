using BookWorm.SharedKernel.Results;

namespace BookWorm.Rating.Features.List;

public sealed class ListFeedbacksEndpoint
    : IEndpoint<Ok<PagedResult<FeedbackDto>>, ListFeedbacksQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                string.Empty,
                async ([AsParameters] ListFeedbacksQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .ProducesGet<PagedResult<FeedbackDto>>(true)
            .WithTags(nameof(Feedback))
            .WithName(nameof(ListFeedbacksEndpoint))
            .WithSummary("List Feedbacks")
            .WithDescription("List feedbacks for a book with pagination and filtering")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<PagedResult<FeedbackDto>>> HandleAsync(
        ListFeedbacksQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}

using BookWorm.Constants.Core;
using BookWorm.SharedKernel.Results;
using Mediator;

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
            .WithPaginationHeaders()
            .WithTags(nameof(Feedback))
            .WithName(nameof(ListFeedbacksEndpoint))
            .WithSummary("List Feedbacks")
            .WithDescription("List feedbacks for a book with pagination and filtering")
            .MapToApiVersion(ApiVersions.V1);
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

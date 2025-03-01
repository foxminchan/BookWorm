using BookWorm.SharedKernel.SeedWork.Model;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.Features.List;

public sealed class ListFeedbacksEndpoint
    : IEndpoint<Ok<PagedResult<FeedbackDto>>, ListFeedbacksQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/feedbacks",
                async ([AsParameters] ListFeedbacksQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<PagedResult<FeedbackDto>>()
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Feedback))
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

using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Rating.Features.Delete;

internal sealed class DeleteFeedbackEndpoint : IEndpoint<NoContent, FeedbackId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{id}",
                async (
                    [Description("The unique identifier of the feedback to be deleted")]
                        FeedbackId id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .ProducesDelete()
            .WithTags(nameof(Feedback))
            .WithName(nameof(DeleteFeedbackEndpoint))
            .WithSummary("Delete Feedback")
            .WithDescription("Delete a feedback if it exists")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        FeedbackId id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteFeedbackCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}

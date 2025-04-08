namespace BookWorm.Rating.Features.Delete;

public sealed class DeleteFeedbackEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/feedbacks/{id:guid}",
                async (
                    [Description("The unique identifier of the feedback to be deleted")] Guid id,
                    ISender sender
                ) => await HandleAsync(id, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Feedback))
            .WithName(nameof(DeleteFeedbackEndpoint))
            .WithSummary("Delete Feedback")
            .WithDescription("Delete a feedback if it exists")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<NoContent> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteFeedbackCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}

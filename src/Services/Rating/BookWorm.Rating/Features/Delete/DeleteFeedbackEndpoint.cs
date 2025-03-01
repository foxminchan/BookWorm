using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.Features.Delete;

public sealed class DeleteFeedbackEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/feedbacks/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Feedback))
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

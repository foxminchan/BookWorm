using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;

namespace BookWorm.Rating.Features.Delete;

public sealed class DeleteFeedbackEndpoint : IEndpoint<NoContent, ObjectId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/feedbacks/{id}",
                async (ObjectId id, ISender sender) => await HandleAsync(id, sender))
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Feedback))
            .WithName("Delete Feedback")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(ObjectId id, ISender sender, CancellationToken cancellationToken = default)
    {
        await sender.Send(new DeleteFeedbackCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}

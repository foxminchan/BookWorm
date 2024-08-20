namespace BookWorm.Rating.Features.Hide;

public sealed class HideFeedbackEndpoint : IEndpoint<Ok, ObjectId, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/feedbacks/{id}/hide",
                async (ObjectId id, ISender sender) => await HandleAsync(id, sender))
            .Produces<Ok>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Feedback))
            .WithName("Hide Feedback")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok> HandleAsync(ObjectId id, ISender sender, CancellationToken cancellationToken = default)
    {
        await sender.Send(new HideFeedbackCommand(id), cancellationToken);

        return TypedResults.Ok();
    }
}

namespace BookWorm.Rating.Features.Create;

public sealed class CreateFeedbackEndpoint
    : IEndpoint<Created<ObjectId>, CreateFeedbackCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/feedbacks",
                async (CreateFeedbackCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Created<ObjectId>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Feedback))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<ObjectId>> HandleAsync(
        CreateFeedbackCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource("Feedbacks").WithId(result.Value).Build(),
            result.Value
        );
    }
}

namespace BookWorm.Rating.Features.Create;

public sealed class CreateFeedbackEndpoint : IEndpoint<Ok<Guid>, CreateFeedbackCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/feedbacks",
                async (CreateFeedbackCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPostWithoutLocation<Guid>()
            .WithTags(nameof(Feedback))
            .WithName(nameof(CreateFeedbackEndpoint))
            .WithSummary("Create Feedback")
            .WithDescription("Create a new feedback")
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Ok<Guid>> HandleAsync(
        CreateFeedbackCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}

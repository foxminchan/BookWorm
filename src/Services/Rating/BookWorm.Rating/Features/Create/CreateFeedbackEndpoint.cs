using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.Features.Create;

public sealed class CreateFeedbackEndpoint
    : IEndpoint<Created<Guid>, CreateFeedbackCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/feedbacks",
                async (CreateFeedbackCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Feedback))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateFeedbackCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource(nameof(Feedback)).WithId(result).Build(),
            result
        );
    }
}

namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed class CreatePublisherEndpoint : IEndpoint<Ok<Guid>, CreatePublisherCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/publishers",
                async (CreatePublisherCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>()
            .ProducesValidationProblem()
            .WithTags(nameof(Publisher))
            .WithName(nameof(CreatePublisherCommand))
            .WithSummary("Create Publisher")
            .WithDescription("Creates a new publisher")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<Ok<Guid>> HandleAsync(
        CreatePublisherCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}

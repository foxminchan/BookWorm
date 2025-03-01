namespace BookWorm.Catalog.Features.Publishers.Create;

public sealed class CreatePublisherEndpoint
    : IEndpoint<Created<Guid>, CreatePublisherCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/publishers",
                async (CreatePublisherCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Publisher))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<Created<Guid>> HandleAsync(
        CreatePublisherCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource(nameof(Publishers)).WithId(result).Build(),
            result
        );
    }
}

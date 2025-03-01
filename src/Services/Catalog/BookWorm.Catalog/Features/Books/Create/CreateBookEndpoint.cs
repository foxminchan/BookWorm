namespace BookWorm.Catalog.Features.Books.Create;

public sealed class CreateBookEndpoint : IEndpoint<Created<Guid>, CreateBookCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/books",
                async ([AsParameters] CreateBookCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .DisableAntiforgery()
            .WithOpenApi()
            .WithTags(nameof(Book))
            .WithFormOptions(true)
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource(nameof(Books)).WithId(result).Build(),
            result
        );
    }
}

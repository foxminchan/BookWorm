using BookWorm.Catalog.Domain;

namespace BookWorm.Catalog.Features.Authors.Create;

public sealed class CreateAuthorEndpoint : IEndpoint<Created<Guid>, CreateAuthorCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/authors",
                async (CreateAuthorCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Created<Guid>>()
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Author))
            .MapToApiVersion(new(1, 0));
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateAuthorCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder()
                .WithVersion()
                .WithResource(nameof(Authors))
                .WithId(result.Value)
                .Build(),
            result.Value
        );
    }
}

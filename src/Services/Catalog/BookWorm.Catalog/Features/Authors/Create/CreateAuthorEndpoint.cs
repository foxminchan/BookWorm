namespace BookWorm.Catalog.Features.Authors.Create;

public sealed class CreateAuthorEndpoint : IEndpoint<Ok<Guid>, CreateAuthorCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/authors",
                async (CreateAuthorCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPostWithoutLocation<Guid>()
            .WithTags(nameof(Author))
            .WithName(nameof(CreateAuthorEndpoint))
            .WithSummary("Create Author")
            .WithDescription("Create a new author in the catalog system")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin);
    }

    public async Task<Ok<Guid>> HandleAsync(
        CreateAuthorCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}

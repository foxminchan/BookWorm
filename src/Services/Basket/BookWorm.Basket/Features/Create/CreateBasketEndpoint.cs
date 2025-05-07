using BookWorm.Basket.Features.Get;

namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketEndpoint
    : IEndpoint<Created<string>, CreateBasketCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/baskets",
                async (CreateBasketCommand command, ISender sender, LinkGenerator linker) =>
                    await HandleAsync(command, sender, linker)
            )
            .ProducesPost<string>()
            .WithTags(nameof(Basket))
            .WithName(nameof(CreateBasketEndpoint))
            .WithSummary("Create Basket")
            .WithDescription("Create a new basket for a user")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<string>> HandleAsync(
        CreateBasketCommand command,
        ISender sender,
        LinkGenerator linker,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        var path = linker.GetPathByName(nameof(GetBasketEndpoint), new { id = result });

        return TypedResults.Created(path, result);
    }
}

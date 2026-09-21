using BookWorm.Basket.Features.Get;
using BookWorm.Constants.Core;
using Mediator;

namespace BookWorm.Basket.Features.Create;

internal sealed class CreateBasketEndpoint
    : IEndpoint<Created<CustomerId>, CreateBasketCommand, ISender, LinkGenerator>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                string.Empty,
                async (CreateBasketCommand command, ISender sender, LinkGenerator linker) =>
                    await HandleAsync(command, sender, linker)
            )
            .ProducesPost<CustomerId>()
            .WithTags(nameof(Basket))
            .WithName(nameof(CreateBasketEndpoint))
            .WithSummary("Create Basket")
            .WithDescription("Create a new basket for a user")
            .MapToApiVersion(ApiVersions.V1)
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<Created<CustomerId>> HandleAsync(
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

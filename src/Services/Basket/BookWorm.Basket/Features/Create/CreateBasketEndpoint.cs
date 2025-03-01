using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketEndpoint : IEndpoint<Created<string>, CreateBasketCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/baskets",
                async (CreateBasketCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Basket))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<string>> HandleAsync(
        CreateBasketCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithResource(nameof(Basket)).WithId(result).Build(),
            result
        );
    }
}

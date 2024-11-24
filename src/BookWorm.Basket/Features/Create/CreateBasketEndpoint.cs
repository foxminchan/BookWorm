namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketEndpoint : IEndpoint<Created<Guid>, CreateBasketCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/baskets",
                async (CreateBasketCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Created<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Basket))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateBasketCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithVersion().WithResource("Baskets").WithId(result.Value).Build(),
            result.Value
        );
    }
}

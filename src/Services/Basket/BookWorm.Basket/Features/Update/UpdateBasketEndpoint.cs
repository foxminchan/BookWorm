namespace BookWorm.Basket.Features.Update;

public sealed class UpdateBasketEndpoint : IEndpoint<NoContent, UpdateBasketCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                string.Empty,
                async (UpdateBasketCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .ProducesPut()
            .WithTags(nameof(Basket))
            .WithName(nameof(UpdateBasketEndpoint))
            .WithSummary("Update Basket")
            .WithDescription("Update a basket by its unique identifier")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization()
            .RequirePerUserRateLimit();
    }

    public async Task<NoContent> HandleAsync(
        UpdateBasketCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}

namespace BookWorm.Basket.Features.RemoveItem;

public sealed class RemoveItemEndpoint : IEndpoint<NoContent, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/baskets/{id:guid}/items",
                async (Guid id, ISender sender) => await HandleAsync(id, sender))
            .Produces<NoContent>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Basket))
            .WithName("Remove Item")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(Guid id, ISender sender, CancellationToken cancellationToken = default)
    {
        await sender.Send(new RemoveItemCommand(id), cancellationToken);

        return TypedResults.NoContent();
    }
}

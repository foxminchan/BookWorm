﻿namespace BookWorm.Basket.Features.ReduceItemQuantity;

public sealed class ReduceItemQuantityEndpoint : IEndpoint<Ok, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/baskets/{id:guid}/reduce-item-quantity",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces<Ok>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Basket))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new ReduceItemQuantityCommand(id), cancellationToken);

        return TypedResults.Ok();
    }
}

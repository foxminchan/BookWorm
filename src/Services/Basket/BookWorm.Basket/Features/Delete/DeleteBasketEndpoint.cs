﻿namespace BookWorm.Basket.Features.Delete;

public sealed class DeleteBasketEndpoint : IEndpoint<NoContent, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/baskets", async (ISender sender) => await HandleAsync(sender))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi()
            .WithTags(nameof(Basket))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<NoContent> HandleAsync(
        ISender request,
        CancellationToken cancellationToken = default
    )
    {
        await request.Send(new DeleteBasketCommand(), cancellationToken);

        return TypedResults.NoContent();
    }
}

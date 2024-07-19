using BookWorm.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Basket.Features.Get;

public sealed class GetBasketEndpoint : IEndpoint<Ok<BasketDto>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/baskets", async (ISender sender) => await HandleAsync(sender))
            .Produces<Ok<BasketDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithTags(nameof(Basket))
            .WithName("Get Basket")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<BasketDto>> HandleAsync(ISender sender, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetBasketQuery(), cancellationToken);

        return TypedResults.Ok(result.Value);
    }
}

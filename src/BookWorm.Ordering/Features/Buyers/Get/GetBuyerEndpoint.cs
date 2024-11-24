using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Features.Buyers.Get;

public sealed class GetBuyerEndpoint : IEndpoint<Ok<BuyerDto>, Guid, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/buyers/{id:guid}",
                async (Guid id, ISender sender) => await HandleAsync(id, sender)
            )
            .Produces<Ok<BuyerDto>>()
            .WithOpenApi()
            .WithTags(nameof(Buyer))
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization();
    }

    public async Task<Ok<BuyerDto>> HandleAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new GetBuyerQuery(id), cancellationToken);

        return TypedResults.Ok(result.Value.ToBuyerDto());
    }
}

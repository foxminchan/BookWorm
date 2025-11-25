using BookWorm.SharedKernel.Results;
using Mediator;

namespace BookWorm.Ordering.Features.Buyers.List;

public sealed class ListBuyersEndpoint
    : IEndpoint<Ok<PagedResult<BuyerDto>>, ListBuyersQuery, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/buyers",
                async ([AsParameters] ListBuyersQuery query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .ProducesGet<PagedResult<BuyerDto>>()
            .WithTags(nameof(Buyer))
            .WithName(nameof(ListBuyersEndpoint))
            .WithSummary("List Buyers")
            .WithDescription("List all buyers with pagination options")
            .MapToApiVersion(new(1, 0))
            .RequireAuthorization(Authorization.Policies.Admin)
            .RequirePerUserRateLimit();
    }

    public async Task<Ok<PagedResult<BuyerDto>>> HandleAsync(
        ListBuyersQuery query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}

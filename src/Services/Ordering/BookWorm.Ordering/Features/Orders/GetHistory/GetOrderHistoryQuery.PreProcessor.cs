namespace BookWorm.Ordering.Features.Orders.GetHistory;

internal sealed class GetOrderHistoryPreProcessor(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : MessagePreProcessor<GetOrderHistoryQuery, IReadOnlyList<OrderEventHistoryDto>>
{
    protected override ValueTask Handle(
        GetOrderHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        return repository.EnsureOrderAccessAsync(request.Id, claimsPrincipal, cancellationToken);
    }
}

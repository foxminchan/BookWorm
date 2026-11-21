namespace BookWorm.Ordering.Features.Orders.GetSummary;

internal sealed class GetOrderSummaryPreProcessor(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : MessagePreProcessor<GetOrderSummaryQuery, OrderSummaryDto>
{
    protected override ValueTask Handle(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken
    )
    {
        return repository.EnsureOrderAccessAsync(request.Id, claimsPrincipal, cancellationToken);
    }
}

using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Orders.GetSummary;

public sealed record GetOrderSummaryQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] OrderId Id,
    [property: Description("Optional stream version for point-in-time replay")]
        long? Version = null,
    [property: Description("Optional UTC timestamp for point-in-time replay")]
        DateTimeOffset? At = null
) : IQuery<OrderSummaryDto>;

internal sealed class GetOrderSummaryHandler(
    IQuerySession querySession,
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : IQueryHandler<GetOrderSummaryQuery, OrderSummaryDto>
{
    public async ValueTask<OrderSummaryDto> Handle(
        GetOrderSummaryQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)request.Id;

        if (!claimsPrincipal.HasRole(Authorization.Roles.Admin))
        {
            var buyerId = claimsPrincipal[ClaimTypes.NameIdentifier].ToBuyerId();
            var order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(orderId, (Guid)buyerId),
                cancellationToken
            );
            Guard.Against.NotFound(order, orderId);
        }

        var summary = await querySession.Events.AggregateStreamAsync<OrderSummary>(
            orderId,
            request.Version ?? 0,
            request.At,
            token: cancellationToken
        );

        Guard.Against.NotFound(summary, orderId);

        var streamState = await querySession.Events.FetchStreamStateAsync(
            orderId,
            cancellationToken
        );

        var version = request.Version ?? streamState?.Version ?? 0;

        return new(summary.Id, summary.Status, summary.TotalPrice, version);
    }
}

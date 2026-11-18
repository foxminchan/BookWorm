using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;

namespace BookWorm.Ordering.Features.Orders.GetHistory;

public sealed record GetOrderHistoryQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] OrderId Id
) : IQuery<IReadOnlyList<OrderEventHistoryDto>>;

internal sealed class GetOrderHistoryHandler(
    IQuerySession querySession,
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal
) : IQueryHandler<GetOrderHistoryQuery, IReadOnlyList<OrderEventHistoryDto>>
{
    public async ValueTask<IReadOnlyList<OrderEventHistoryDto>> Handle(
        GetOrderHistoryQuery request,
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

        var events = await querySession.Events.FetchStreamAsync(orderId, token: cancellationToken);

        if (events.Count == 0)
        {
            throw new NotFoundException($"Order event stream with id {orderId} not found.");
        }

        return
        [
            .. events.Select(e =>
            {
                var (status, totalPrice) = e.Data switch
                {
                    OrderPlacedEvent placed => (Status.New, placed.Order.TotalPrice),
                    OrderCompletedEvent completed => (Status.Completed, completed.Order.TotalPrice),
                    OrderCancelledEvent cancelled => (Status.Cancelled, cancelled.Order.TotalPrice),
                    _ => ((Status?)null, (decimal?)null),
                };

                return new OrderEventHistoryDto(
                    e.Sequence,
                    e.Version,
                    e.EventTypeName,
                    e.Timestamp,
                    status,
                    totalPrice
                );
            }),
        ];
    }
}

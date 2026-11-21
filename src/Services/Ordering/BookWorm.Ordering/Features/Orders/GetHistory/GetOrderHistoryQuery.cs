using JasperFx.Events;

namespace BookWorm.Ordering.Features.Orders.GetHistory;

public sealed record GetOrderHistoryQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] OrderId Id
) : IQuery<IReadOnlyList<OrderEventHistoryDto>>;

internal sealed class GetOrderHistoryHandler(IQuerySession querySession)
    : IQueryHandler<GetOrderHistoryQuery, IReadOnlyList<OrderEventHistoryDto>>
{
    public async ValueTask<IReadOnlyList<OrderEventHistoryDto>> Handle(
        GetOrderHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)request.Id;

        var events = await querySession.Events.FetchStreamAsync(orderId, token: cancellationToken);

        return events.Count == 0
            ? throw new NotFoundException($"Order event stream with id {orderId} not found.")
            : [.. events.Select(MapToDto)];
    }

    private static OrderEventHistoryDto MapToDto(IEvent @event)
    {
        var (status, totalPrice) = @event.Data switch
        {
            OrderPlacedEvent placed => (Status.New, placed.Order.TotalPrice),
            OrderCompletedEvent completed => (Status.Completed, completed.Order.TotalPrice),
            OrderCancelledEvent cancelled => (Status.Cancelled, cancelled.Order.TotalPrice),
            _ => ((Status?)null, (decimal?)null),
        };

        return new(
            @event.Sequence,
            @event.Version,
            @event.EventTypeName,
            @event.Timestamp,
            status,
            totalPrice
        );
    }
}

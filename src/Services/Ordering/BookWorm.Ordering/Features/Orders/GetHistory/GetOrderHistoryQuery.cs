using JasperFx.Events;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.Features.Orders.GetHistory;

public sealed record GetOrderHistoryQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] OrderId Id
) : IQuery<IReadOnlyList<OrderEventHistoryDto>>;

internal sealed class GetOrderHistoryHandler(IQuerySession querySession, IFusionCache cache)
    : IQueryHandler<GetOrderHistoryQuery, IReadOnlyList<OrderEventHistoryDto>>
{
    private static readonly FusionCacheEntryOptions _immutableCacheOptions = new()
    {
        Duration = TimeSpan.FromHours(1),
        DistributedCacheDuration = TimeSpan.FromHours(1),
    };

    public async ValueTask<IReadOnlyList<OrderEventHistoryDto>> Handle(
        GetOrderHistoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderId = (Guid)request.Id;
        var key = OrderingCacheKeys.OrderHistoryTerminal(orderId);

        var cached = await cache.TryGetAsync<IReadOnlyList<OrderEventHistoryDto>>(
            key,
            token: cancellationToken
        );
        if (cached.HasValue && cached.Value is not null)
        {
            return cached.Value;
        }

        var events = await querySession.Events.FetchStreamAsync(orderId, token: cancellationToken);

        if (events.Count == 0)
        {
            throw new NotFoundException($"Order event stream with id {orderId} not found.");
        }

        IReadOnlyList<OrderEventHistoryDto> history = [.. events.Select(MapToDto)];

        if (history.Any(e => e.Status is Status.Completed or Status.Cancelled))
        {
            await cache.SetAsync(key, history, _immutableCacheOptions, cancellationToken);
        }

        return history;
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

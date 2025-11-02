using BookWorm.Chassis.Logging;

namespace BookWorm.Ordering.Extensions;

internal static partial class OrderingTrace
{
    [LoggerMessage(
        EventId = 0,
        EventName = nameof(OrderPlacedEvent),
        Level = LogLevel.Information,
        Message = "Order with Id {OrderId} placed with status {Status}"
    )]
    public static partial void LogOrderPlaced(
        ILogger logger,
        [SensitiveData] Guid orderId,
        Status status
    );

    [LoggerMessage(
        EventId = 1,
        EventName = nameof(OrderCancelledEvent),
        Level = LogLevel.Information,
        Message = "Order with Id {OrderId} cancelled with status {Status}"
    )]
    public static partial void LogOrderCancelled(
        ILogger logger,
        [SensitiveData] Guid orderId,
        Status status
    );

    [LoggerMessage(
        EventId = 2,
        EventName = nameof(OrderCompletedEvent),
        Level = LogLevel.Information,
        Message = "Order with Id {OrderId} cancelled with status {Status}"
    )]
    public static partial void LogOrderCompleted(
        ILogger logger,
        [SensitiveData] Guid orderId,
        Status status
    );
}

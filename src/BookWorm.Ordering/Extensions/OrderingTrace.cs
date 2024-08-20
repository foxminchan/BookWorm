namespace BookWorm.Ordering.Extensions;

internal static partial class OrderingTrace
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "[[{Event}] - Order {OrderId} created")]
    public static partial void LogOrderCreated(ILogger logger, string? @event, Guid orderId);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[[{Event}] - Order {OrderId} completed")]
    public static partial void LogOrderCompleted(ILogger logger, string? @event, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[[{Event}] - Order {OrderId} cancelled")]
    public static partial void LogOrderCancelled(ILogger logger, string? @event, Guid orderId);
}

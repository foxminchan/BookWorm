namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderEventHistoryDto(
    long Sequence,
    long Version,
    string EventType,
    DateTimeOffset Timestamp,
    Status? Status,
    decimal? TotalPrice
);

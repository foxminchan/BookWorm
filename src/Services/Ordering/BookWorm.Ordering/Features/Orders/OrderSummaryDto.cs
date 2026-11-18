namespace BookWorm.Ordering.Features.Orders;

public sealed record OrderSummaryDto(Guid Id, Status Status, decimal TotalPrice, long Version)
    : IHasVersion;

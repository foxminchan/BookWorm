namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public sealed record OrderDashboardSummary(
    long TotalOrders,
    decimal CompletedRevenue,
    IReadOnlyList<DailyOrderSummary> DailyOrders,
    IReadOnlyList<RecentOrderSummary> RecentOrders
);

public sealed record DailyOrderSummary(DateOnly Date, long Orders, decimal Revenue);

public sealed record RecentOrderSummary(
    OrderId Id,
    DateTime CreatedAt,
    decimal Total,
    Status Status
);

public sealed record DashboardOrder
{
    public OrderId Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public Status Status { get; init; }
    public decimal Total { get; init; }
}

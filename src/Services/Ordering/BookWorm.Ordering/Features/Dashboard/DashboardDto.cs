namespace BookWorm.Ordering.Features.Dashboard;

public sealed record DashboardDto(
    long TotalOrders,
    decimal TotalRevenue,
    long TotalCustomers,
    long TotalBooks,
    IReadOnlyList<DailyOrdersDto> DailyOrders,
    IReadOnlyList<CategoryCountDto> Categories,
    IReadOnlyList<RecentOrderDto> RecentOrders,
    DateTimeOffset UpdatedAt
);

public sealed record DailyOrdersDto(DateOnly Date, long Orders, decimal Revenue);

public sealed record RecentOrderDto(Guid Id, DateTimeOffset Date, decimal Total, string Status);

public sealed record CategoryCountDto(string Name, long Value);

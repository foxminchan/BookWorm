using Mediator;
using ZiggyCreatures.Caching.Fusion;

namespace BookWorm.Ordering.Features.Dashboard;

public sealed record GetDashboardQuery : IQuery<DashboardDto>;

internal sealed class GetDashboardHandler(
    IOrderRepository orderRepository,
    IBuyerRepository buyerRepository,
    IFusionCache cache,
    IBookService bookService
) : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    public async ValueTask<DashboardDto> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken
    )
    {
        return await cache.GetOrSetAsync(
            "ordering:dashboard:v1",
            async ct =>
            {
                var catalog = await bookService.GetDashboardAsync(ct);
                var now = DateTimeOffset.UtcNow;
                var today = DateOnly.FromDateTime(now.UtcDateTime);
                var start = today.AddDays(-6).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var end = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var orders = await orderRepository.GetDashboardAsync(start, end, ct);
                var totalCustomers = await buyerRepository.CountAsync(ct);

                return new DashboardDto(
                    orders.TotalOrders,
                    orders.CompletedRevenue,
                    totalCustomers,
                    catalog.TotalBooks,
                    FillDailyOrders(
                        today,
                        [
                            .. orders.DailyOrders.Select(day => new DailyOrdersDto(
                                day.Date,
                                day.Orders,
                                day.Revenue
                            )),
                        ]
                    ),
                    [
                        .. catalog.Categories.Select(category => new CategoryCountDto(
                            category.Name,
                            category.Value
                        )),
                    ],
                    [
                        .. orders.RecentOrders.Select(order => new RecentOrderDto(
                            (Guid)order.Id,
                            new(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
                            order.Total,
                            order.Status.ToString()
                        )),
                    ],
                    now
                );
            },
            new FusionCacheEntryOptions { Duration = TimeSpan.FromSeconds(15) },
            cancellationToken
        );
    }

    private static IReadOnlyList<DailyOrdersDto> FillDailyOrders(
        DateOnly today,
        IReadOnlyList<DailyOrdersDto> dailyOrders
    )
    {
        var byDate = dailyOrders.ToDictionary(day => day.Date);
        return
        [
            .. Enumerable
                .Range(-6, 7)
                .Select(offset =>
                {
                    var date = today.AddDays(offset);
                    return byDate.GetValueOrDefault(date) ?? new DailyOrdersDto(date, 0, 0);
                }),
        ];
    }
}

using BookWorm.Chassis.Specification.Evaluators;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;

namespace BookWorm.Ordering.Infrastructure.Repositories;

internal sealed class OrderRepository(OrderingDbContext context) : IOrderRepository
{
    private readonly OrderingDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken)
    {
        var entry = await _context.Orders.AddAsync(order, cancellationToken);
        return entry.Entity;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders.FindAsync([OrderId.From(orderId)], cancellationToken);
    }

    public async Task<Order?> FirstOrDefaultAsync(
        ISpecification<Order> spec,
        CancellationToken cancellationToken
    )
    {
        return await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            Specification.GetQuery(_context.Orders, spec),
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<Order>> ListAsync(
        ISpecification<Order> spec,
        CancellationToken cancellationToken
    )
    {
        return await EntityFrameworkQueryableExtensions.ToListAsync(
            Specification.GetQuery(_context.Orders, spec),
            cancellationToken
        );
    }

    public async Task<long> CountAsync(
        ISpecification<Order> spec,
        CancellationToken cancellationToken
    )
    {
        return await EntityFrameworkQueryableExtensions.LongCountAsync(
            Specification.GetQuery(_context.Orders, spec),
            cancellationToken
        );
    }

    public void Delete(Order order)
    {
        _context.Orders.Remove(order);
    }

    public async Task<OrderDashboardSummary> GetDashboardAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken
    )
    {
        var totalOrders = await CountAsync(new OrderFilterSpec(), cancellationToken);
        var completedRevenue = await GetDashboardOrders(new OrderFilterSpec(Status.Completed))
            .SumAsync(order => order.Total, cancellationToken);
        var dailyOrders = await EntityFrameworkQueryableExtensions.ToListAsync(
            GetDashboardOrders(new OrderDateRangeSpec(start, end))
                .GroupBy(order => order.CreatedAt.Date)
                .Select(group => new DailyOrderSummary(
                    DateOnly.FromDateTime(group.Key),
                    group.LongCount(),
                    group.Sum(order => order.Status == Status.Completed ? order.Total : 0)
                )),
            cancellationToken
        );
        var recentOrders = await EntityFrameworkQueryableExtensions.ToListAsync(
            GetDashboardOrders(new RecentOrdersSpec(5))
                .Select(order => new RecentOrderSummary(
                    order.Id,
                    order.CreatedAt,
                    order.Total,
                    order.Status
                )),
            cancellationToken
        );
        return new(totalOrders, completedRevenue, dailyOrders, recentOrders);
    }

    private IQueryable<DashboardOrder> GetDashboardOrders(ISpecification<Order> spec)
    {
        return Specification
            .GetQuery(_context.Orders.IgnoreAutoIncludes(), spec)
            .Select(order => new DashboardOrder
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                Total = order.OrderItems.Sum(item => item.Price * item.Quantity),
            });
    }
}

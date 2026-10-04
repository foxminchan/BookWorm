namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public interface IOrderRepository : IRepository<Order>
{
    Task<OrderDashboardSummary> GetDashboardAsync(
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken
    );

    Task<Order> AddAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<Order?> FirstOrDefaultAsync(
        ISpecification<Order> spec,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<Order>> ListAsync(
        ISpecification<Order> spec,
        CancellationToken cancellationToken
    );

    Task<long> CountAsync(ISpecification<Order> spec, CancellationToken cancellationToken);
    void Delete(Order order);
}

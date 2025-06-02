namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public interface IOrderRepository : IRepository<Order>
{
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

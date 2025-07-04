using BookWorm.Chassis.Specification.Evaluators;

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
        return await _context.Orders.FindAsync([orderId], cancellationToken);
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
}

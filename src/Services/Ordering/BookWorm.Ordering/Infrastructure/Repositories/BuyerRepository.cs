using BookWorm.Chassis.Specification.Evaluators;

namespace BookWorm.Ordering.Infrastructure.Repositories;

public sealed class BuyerRepository(OrderingDbContext context) : IBuyerRepository
{
    private readonly OrderingDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Buyers.FindAsync([id], cancellationToken);
    }

    public async Task<Buyer> AddAsync(Buyer buyer, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Buyers.AddAsync(buyer, cancellationToken);
        return entry.Entity;
    }

    public async Task<IReadOnlyList<Buyer>> ListAsync(
        ISpecification<Buyer> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await EntityFrameworkQueryableExtensions.ToListAsync(
            Specification.GetQuery(_context.Buyers, spec),
            cancellationToken
        );
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await EntityFrameworkQueryableExtensions.LongCountAsync(
            _context.Buyers.AsNoTracking(),
            cancellationToken
        );
    }

    public void Delete(Buyer buyer)
    {
        _context.Buyers.Remove(buyer);
    }
}

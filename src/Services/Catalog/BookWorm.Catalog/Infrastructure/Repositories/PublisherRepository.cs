namespace BookWorm.Catalog.Infrastructure.Repositories;

public sealed class PublisherRepository(CatalogDbContext context) : IPublisherRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Publisher> AddAsync(Publisher publisher, CancellationToken cancellationToken)
    {
        var result = await _context.Publishers.AddAsync(publisher, cancellationToken);

        return result.Entity;
    }

    public async Task<IReadOnlyList<Publisher>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.Publishers.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Publisher?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Publishers.FindAsync([id], cancellationToken);
    }
}

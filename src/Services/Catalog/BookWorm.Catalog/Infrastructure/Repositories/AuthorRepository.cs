using BookWorm.Chassis.Specification.Evaluators;

namespace BookWorm.Catalog.Infrastructure.Repositories;

internal sealed class AuthorRepository(CatalogDbContext context) : IAuthorRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Author> AddAsync(Author author, CancellationToken cancellationToken)
    {
        var entry = await _context.Authors.AddAsync(author, cancellationToken);
        return entry.Entity;
    }

    public async Task<IReadOnlyList<Author>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.Authors.ToListAsync(cancellationToken);
    }

    public void Delete(Author author)
    {
        _context.Authors.Remove(author);
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Authors.FindAsync([id], cancellationToken);
    }

    public async Task<Author?> FirstOrDefaultAsync(
        ISpecification<Author> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(_context.Authors.AsQueryable(), spec)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

using BookWorm.SharedKernel.Specification.Evaluators;

namespace BookWorm.Catalog.Infrastructure.Repositories;

public sealed class BookRepository(CatalogDbContext context) : IBookRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Books.AddAsync(book, cancellationToken);
        return entry.Entity;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Books.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification.GetQuery(_context.Books, spec).ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification.GetQuery(_context.Books, spec).LongCountAsync(cancellationToken);
    }
}

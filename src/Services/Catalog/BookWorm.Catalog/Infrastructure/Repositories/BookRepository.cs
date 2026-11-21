using BookWorm.Chassis.Specification.Evaluators;

namespace BookWorm.Catalog.Infrastructure.Repositories;

internal sealed class BookRepository(CatalogDbContext context) : IBookRepository
{
    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => context;

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        var entry = await context.Books.AddAsync(book, cancellationToken);
        return entry.Entity;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Books.FindAsync([id], cancellationToken);
    }

    public async Task<BookSummary?> GetSummaryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var bookId = BookId.From(id);
        return await context
            .Books.IgnoreAutoIncludes()
            .AsNoTracking()
            .Where(book => book.Id == bookId)
            .Select(book => new BookSummary(
                book.Id,
                book.Name,
                book.Price != null ? book.Price.OriginalPrice : null,
                book.Price != null ? book.Price.DiscountPrice : null,
                book.Status
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookSummary>> ListSummariesAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(context.Books.IgnoreAutoIncludes().AsNoTracking(), spec)
            .Select(book => new BookSummary(
                book.Id,
                book.Name,
                book.Price != null ? book.Price.OriginalPrice : null,
                book.Price != null ? book.Price.DiscountPrice : null,
                book.Status
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> ListAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification.GetQuery(context.Books, spec).ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification.GetQuery(context.Books, spec).LongCountAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(context.Books.IgnoreAutoIncludes().AsNoTracking(), spec)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BookCategoryCount>> CountByCategoryAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .Books.AsNoTracking()
            .GroupBy(book => book.Category == null ? "Other" : book.Category.Name ?? "Other")
            .OrderBy(group => group.Key)
            .Select(group => new BookCategoryCount(group.Key, group.LongCount()))
            .ToListAsync(cancellationToken);
    }
}

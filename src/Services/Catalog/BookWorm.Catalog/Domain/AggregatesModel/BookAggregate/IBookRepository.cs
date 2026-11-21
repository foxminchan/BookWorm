namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public interface IBookRepository : IRepository<Book>
{
    Task<IReadOnlyList<BookCategoryCount>> CountByCategoryAsync(
        CancellationToken cancellationToken = default
    );

    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BookSummary?> GetSummaryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookSummary>> ListSummariesAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Book>> ListAsync(
        ISpecification<Book> spec,
        CancellationToken cancellationToken = default
    );

    Task<long> CountAsync(ISpecification<Book> spec, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(ISpecification<Book> spec, CancellationToken cancellationToken = default);
}

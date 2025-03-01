namespace BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;

public interface IAuthorRepository : IRepository<Author>
{
    Task<Author> AddAsync(Author author, CancellationToken cancellationToken);
    Task<IReadOnlyList<Author>> ListAsync(CancellationToken cancellationToken);
    void Delete(Author author);
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Author?> FirstOrDefaultAsync(
        ISpecification<Author> spec,
        CancellationToken cancellationToken = default
    );
}

using BookWorm.Chassis.Specification.Evaluators;

namespace BookWorm.Catalog.Infrastructure.Repositories;

internal sealed class AuthorRepository(CatalogDbContext context) : IAuthorRepository
{
    private static SpecificationEvaluator Specification => SpecificationEvaluator.Instance;

    public IUnitOfWork UnitOfWork => context;

    public async Task<Author> AddAsync(Author author, CancellationToken cancellationToken)
    {
        var entry = await context.Authors.AddAsync(author, cancellationToken);
        return entry.Entity;
    }

    public async Task<IReadOnlyList<Author>> ListAsync(CancellationToken cancellationToken)
    {
        return await context
            .Authors.AsNoTracking()
            .Take(Pagination.DefaultQueryLimit)
            .ToListAsync(cancellationToken);
    }

    public void Delete(Author author)
    {
        context.Authors.Remove(author);
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Authors.FindAsync([id], cancellationToken);
    }

    public async Task<Author?> FirstOrDefaultAsync(
        ISpecification<Author> spec,
        CancellationToken cancellationToken = default
    )
    {
        return await Specification
            .GetQuery(context.Authors.AsQueryable(), spec)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

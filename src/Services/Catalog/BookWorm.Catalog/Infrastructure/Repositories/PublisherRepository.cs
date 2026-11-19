namespace BookWorm.Catalog.Infrastructure.Repositories;

internal sealed class PublisherRepository(CatalogDbContext context) : IPublisherRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Publisher> AddAsync(Publisher publisher, CancellationToken cancellationToken)
    {
        var result = await context.Publishers.AddAsync(publisher, cancellationToken);

        return result.Entity;
    }

    public async Task<IReadOnlyList<Publisher>> ListAsync(CancellationToken cancellationToken)
    {
        return await context
            .Publishers.AsNoTracking()
            .Take(Pagination.DefaultQueryLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Publisher?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Publishers.FindAsync([id], cancellationToken);
    }

    public void Delete(Publisher publisher)
    {
        context.Publishers.Remove(publisher);
    }
}

namespace BookWorm.Catalog.Infrastructure.Repositories;

internal sealed class CategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Category> AddAsync(
        Category category,
        CancellationToken cancellationToken = default
    )
    {
        await context.Categories.AddAsync(category, cancellationToken);
        return category;
    }

    public async Task<IReadOnlyList<Category>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .Categories.AsNoTracking()
            .Take(Pagination.DefaultQueryLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await context.Categories.FindAsync([id], cancellationToken);
    }

    public void Delete(Category category)
    {
        context.Categories.Remove(category);
    }
}

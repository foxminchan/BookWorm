namespace BookWorm.Catalog.Infrastructure.Repositories;

public sealed class CategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Category> AddAsync(
        Category category,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Categories.AddAsync(category, cancellationToken);
        return category;
    }

    public async Task<IReadOnlyList<Category>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Categories.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Categories.FindAsync([id], cancellationToken);
    }
}

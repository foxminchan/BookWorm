namespace BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Delete(Category category);
}

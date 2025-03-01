namespace BookWorm.Catalog.Infrastructure.Services;

public interface IBookSemanticSearch
{
    Task<Guid[]> FindBooksAsync(string searchText, CancellationToken cancellationToken = default);
}

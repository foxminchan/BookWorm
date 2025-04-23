namespace BookWorm.Catalog.Infrastructure.GenAi.SemanticSearch;

public interface ISemanticSearch
{
    Task<IReadOnlyList<SemanticSearchRecord>> FindAsync(
        string text,
        string collectionName,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    );
}

namespace BookWorm.Chassis.Search;

public interface ISearch
{
    Task<IReadOnlyList<HybridSearchRecord>> SearchAsync(
        string text,
        ICollection<string> keywords,
        string collectionName,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    );
}

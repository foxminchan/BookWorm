namespace BookWorm.Chassis.AI.Search;

public interface ISearch
{
    Task<IReadOnlyList<TextSnippet>> SearchAsync(
        string text,
        ICollection<string> keywords,
        int maxResults = 20,
        CancellationToken cancellationToken = default
    );
}

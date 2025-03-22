using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class SearchExpression<T>
    where T : class
{
    public SearchExpression(
        Expression<Func<T, string?>> selector,
        string searchTerm,
        int searchGroup = 1
    )
    {
        _ = selector ?? throw new ArgumentNullException(nameof(selector));

        if (string.IsNullOrEmpty(searchTerm))
        {
            throw new ArgumentException("Search term cannot be null or empty.", nameof(searchTerm));
        }

        Selector = selector;
        SearchTerm = searchTerm;
        SearchGroup = searchGroup;
    }

    public Expression<Func<T, string?>> Selector { get; }

    public string SearchTerm { get; }

    public int SearchGroup { get; }
}

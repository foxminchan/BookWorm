using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class SearchExpression<T>(
    Expression<Func<T, string>> selector,
    string searchTerm,
    int searchGroup = 1
)
    where T : class
{
    private readonly Lazy<Func<T, string>> _selectorFunc = new(selector.Compile);

    public Expression<Func<T, string>> Selector { get; } = selector;

    public string SearchTerm { get; } = searchTerm;

    public int SearchGroup { get; } = searchGroup;

    public Func<T, string> SelectorFunc => _selectorFunc.Value;
}

using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class WhereExpression<T>
    where T : class
{
    private readonly Lazy<Func<T, bool>> _filterFunc;

    public WhereExpression(Expression<Func<T, bool>> filter)
    {
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        Filter = filter;

        _filterFunc = new(Filter.Compile);
    }

    public Expression<Func<T, bool>> Filter { get; }

    public Func<T, bool> FilterFunc => _filterFunc.Value;
}

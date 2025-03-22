using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public sealed class WhereExpression<T>
    where T : class
{
    public WhereExpression(Expression<Func<T, bool>> filter)
    {
        _ = filter ?? throw new ArgumentNullException(nameof(filter));

        Filter = filter;
    }

    public Expression<Func<T, bool>> Filter { get; }
}

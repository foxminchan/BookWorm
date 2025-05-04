using System.Linq.Expressions;
using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> Search<T>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, string?>> keySelector,
        string pattern,
        int group = 1
    )
        where T : class
    {
        var expr = new SearchExpression<T>(keySelector, pattern, group);
        builder.Specification.Add(expr);
        return builder;
    }
}

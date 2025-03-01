using System.Linq.Expressions;
using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> OrderBy<T>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, object?>> keySelector
    )
        where T : class
    {
        var expr = new OrderExpression<T>(keySelector, OrderType.OrderBy);
        builder.Specification.Add(expr);
        return builder;
    }

    public static ISpecificationBuilder<T> OrderByDescending<T>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, object?>> keySelector
    )
        where T : class
    {
        var expr = new OrderExpression<T>(keySelector, OrderType.OrderByDescending);
        builder.Specification.Add(expr);
        return builder;
    }

    public static ISpecificationBuilder<T> ThenBy<T>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, object?>> keySelector
    )
        where T : class
    {
        var expr = new OrderExpression<T>(keySelector, OrderType.ThenBy);
        builder.Specification.Add(expr);
        return builder;
    }

    public static ISpecificationBuilder<T> ThenByDescending<T>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, object?>> keySelector
    )
        where T : class
    {
        var expr = new OrderExpression<T>(keySelector, OrderType.ThenByDescending);
        builder.Specification.Add(expr);
        return builder;
    }
}

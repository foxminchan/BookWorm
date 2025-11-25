using System.Linq.Expressions;
using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static IIncludeSpecificationBuilder<TEntity, TProperty> ThenInclude<
        TEntity,
        TPreviousProperty,
        TProperty
    >(
        this IIncludeSpecificationBuilder<TEntity, TPreviousProperty> builder,
        Expression<Func<TPreviousProperty, TProperty>> navigationSelector
    )
        where TEntity : class
    {
        var expr = new IncludeExpression(navigationSelector, typeof(TPreviousProperty));
        builder.Specification.Add(expr);

        var includeBuilder = new IncludeSpecificationBuilder<TEntity, TProperty>(
            builder.Specification
        );
        return includeBuilder;
    }

    public static IIncludeSpecificationBuilder<TEntity, TProperty> ThenInclude<
        TEntity,
        TPreviousProperty,
        TProperty
    >(
        this IIncludeSpecificationBuilder<TEntity, IEnumerable<TPreviousProperty>> builder,
        Expression<Func<TPreviousProperty, TProperty>> navigationSelector
    )
        where TEntity : class
    {
        var expr = new IncludeExpression(
            navigationSelector,
            typeof(IEnumerable<TPreviousProperty>)
        );
        builder.Specification.Add(expr);

        var includeBuilder = new IncludeSpecificationBuilder<TEntity, TProperty>(
            builder.Specification
        );
        return includeBuilder;
    }

    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> Include(string includeString)
        {
            builder.Specification.Add(includeString);
            return builder;
        }

        public IIncludeSpecificationBuilder<T, TProperty> Include<TProperty>(
            Expression<Func<T, TProperty>> navigationSelector
        )
        {
            var expr = new IncludeExpression(navigationSelector);
            builder.Specification.Add(expr);

            var includeBuilder = new IncludeSpecificationBuilder<T, TProperty>(
                builder.Specification
            );
            return includeBuilder;
        }
    }
}

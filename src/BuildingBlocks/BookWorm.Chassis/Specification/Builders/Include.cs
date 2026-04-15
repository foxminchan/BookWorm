using System.Linq.Expressions;
using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<TEntity, TPreviousProperty>(
        IIncludeSpecificationBuilder<TEntity, TPreviousProperty> builder
    )
        where TEntity : class
    {
        public IIncludeSpecificationBuilder<TEntity, TProperty> ThenInclude<TProperty>(
            Expression<Func<TPreviousProperty, TProperty>> navigationSelector
        )
        {
            var expr = new IncludeExpression(navigationSelector, typeof(TPreviousProperty));
            builder.Specification.Add(expr);

            var includeBuilder = new IncludeSpecificationBuilder<TEntity, TProperty>(
                builder.Specification
            );
            return includeBuilder;
        }
    }

    extension<TEntity, TPreviousProperty>(
        IIncludeSpecificationBuilder<TEntity, IEnumerable<TPreviousProperty>> builder
    )
        where TEntity : class
    {
        public IIncludeSpecificationBuilder<TEntity, TProperty> ThenInclude<TProperty>(
            Expression<Func<TPreviousProperty, TProperty>> navigationSelector
        )
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

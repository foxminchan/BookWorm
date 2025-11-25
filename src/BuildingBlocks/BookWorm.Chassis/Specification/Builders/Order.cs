using System.Linq.Expressions;
using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> OrderBy(Expression<Func<T, object?>> keySelector)
        {
            var expr = new OrderExpression<T>(keySelector, OrderType.OrderBy);
            builder.Specification.Add(expr);
            return builder;
        }

        public ISpecificationBuilder<T> OrderByDescending(Expression<Func<T, object?>> keySelector)
        {
            var expr = new OrderExpression<T>(keySelector, OrderType.OrderByDescending);
            builder.Specification.Add(expr);
            return builder;
        }

        public ISpecificationBuilder<T> ThenBy(Expression<Func<T, object?>> keySelector)
        {
            var expr = new OrderExpression<T>(keySelector, OrderType.ThenBy);
            builder.Specification.Add(expr);
            return builder;
        }

        public ISpecificationBuilder<T> ThenByDescending(Expression<Func<T, object?>> keySelector)
        {
            var expr = new OrderExpression<T>(keySelector, OrderType.ThenByDescending);
            builder.Specification.Add(expr);
            return builder;
        }
    }
}

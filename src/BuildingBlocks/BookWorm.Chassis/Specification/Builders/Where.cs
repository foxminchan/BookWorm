using System.Linq.Expressions;
using BookWorm.Chassis.Specification.Expressions;

namespace BookWorm.Chassis.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    extension<T>(ISpecificationBuilder<T> builder)
        where T : class
    {
        public ISpecificationBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            var expr = new WhereExpression<T>(predicate);
            builder.Specification.Add(expr);
            return builder;
        }
    }
}

using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification;

public interface ISpecification<T>
    where T : class
{
    IEnumerable<OrderExpression<T>>? OrderExpressions { get; }

    IEnumerable<WhereExpression<T>>? WhereExpressions { get; }

    IEnumerable<SearchExpression<T>> SearchExpressions { get; }

    IEnumerable<IncludeExpression> IncludeExpressions { get; }

    IEnumerable<string> IncludeStrings { get; }

    int Take { get; }

    int Skip { get; }

    bool AsNoTracking { get; }

    bool AsSplitQuery { get; }

    bool IgnoreQueryFilters { get; }
}

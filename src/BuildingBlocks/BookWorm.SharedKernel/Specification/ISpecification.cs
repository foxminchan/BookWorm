using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification;

public interface ISpecification<T>
    where T : class
{
    IEnumerable<OrderExpression<T>>? OrderBy { get; }

    IEnumerable<WhereExpression<T>>? Where { get; }

    IEnumerable<SearchExpression<T>> SearchCriteria { get; }

    IEnumerable<IncludeExpression> IncludesExpression { get; }

    IEnumerable<string> IncludeStrings { get; }

    int Take { get; }

    int Skip { get; }

    bool AsNoTracking { get; }

    bool AsSplitQuery { get; }

    bool IgnoreQueryFilters { get; }
}

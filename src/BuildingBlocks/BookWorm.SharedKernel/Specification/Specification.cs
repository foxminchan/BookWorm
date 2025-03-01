using System.Linq.Expressions;
using BookWorm.SharedKernel.Specification.Builders;
using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification;

public class Specification<T> : ISpecification<T>
    where T : class
{
    private List<IncludeExpression>? _includeExpressions;
    private List<Expression<Func<T, object>>>? _includes;
    private List<string>? _includeStrings;
    private List<OrderExpression<T>>? _orderBy;
    private List<SearchExpression<T>>? _searchCriteria;
    private List<Expression<Func<T, object>>>? _thenIncludes;
    private List<WhereExpression<T>>? _where;

    protected ISpecificationBuilder<T> Query => new SpecificationBuilder<T>(this);
    public IEnumerable<OrderExpression<T>> OrderBy => _orderBy ?? [];
    public IEnumerable<WhereExpression<T>> Where => _where ?? [];
    public IEnumerable<SearchExpression<T>> SearchCriteria => _searchCriteria ?? [];
    public IEnumerable<IncludeExpression> IncludesExpression => _includeExpressions ?? [];
    public IEnumerable<string> IncludeStrings => _includeStrings ?? [];

    public int Take { get; set; }
    public int Skip { get; set; }
    public bool AsNoTracking { get; internal set; } = false;
    public bool AsSplitQuery { get; internal set; } = false;
    public bool IgnoreQueryFilters { get; internal set; } = false;

    internal void Add(OrderExpression<T> orderExpression)
    {
        _orderBy ??= new(2);
        _orderBy.Add(orderExpression);
    }

    internal void Add(WhereExpression<T> whereExpression)
    {
        _where ??= new(2);
        _where.Add(whereExpression);
    }

    internal void Add(SearchExpression<T> searchExpression)
    {
        _searchCriteria ??= new(2);
        _searchCriteria.Add(searchExpression);
    }

    internal void Add(Expression<Func<T, object>> includeExpression)
    {
        _includes ??= new(2);
        _includes.Add(includeExpression);
    }

    internal void ThenInclude(Expression<Func<T, object>> thenIncludeExpression)
    {
        _thenIncludes ??= new(2);
        _thenIncludes.Add(thenIncludeExpression);
    }

    internal void Add(string includeString)
    {
        _includeStrings ??= new(2);
        _includeStrings.Add(includeString);
    }

    internal void Add(IncludeExpression includeExpression)
    {
        _includeExpressions ??= new(2);
        _includeExpressions.Add(includeExpression);
    }
}

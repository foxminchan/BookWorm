using BookWorm.SharedKernel.Specification.Builders;
using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification;

public class Specification<T> : ISpecification<T>
    where T : class
{
    private const int DefaultCapacityWhere = 2;
    private const int DefaultCapacitySearch = 2;
    private const int DefaultCapacityOrder = 2;
    private const int DefaultCapacityInclude = 2;
    private const int DefaultCapacityIncludeString = 1;

    private List<WhereExpression<T>>? _whereExpressions;
    private List<SearchExpression<T>>? _searchExpressions;
    private List<OrderExpression<T>>? _orderExpressions;
    private List<IncludeExpression>? _includeExpressions;
    private List<string>? _includeStrings;

    protected ISpecificationBuilder<T> Query => new SpecificationBuilder<T>(this);
    public IEnumerable<WhereExpression<T>> WhereExpressions => _whereExpressions ?? [];
    public IEnumerable<SearchExpression<T>> SearchExpressions => _searchExpressions ?? [];
    public IEnumerable<OrderExpression<T>> OrderExpressions => _orderExpressions ?? [];
    public IEnumerable<IncludeExpression> IncludeExpressions => _includeExpressions ?? [];
    public IEnumerable<string> IncludeStrings => _includeStrings ?? [];

    public int Take { get; set; }
    public int Skip { get; set; }
    public bool AsNoTracking { get; internal set; }
    public bool AsSplitQuery { get; internal set; }
    public bool IgnoreQueryFilters { get; internal set; }

    internal void Add(WhereExpression<T> whereExpressions)
    {
        _whereExpressions ??= new(DefaultCapacityWhere);
        _whereExpressions.Add(whereExpressions);
    }

    internal void Add(SearchExpression<T> searchExpressions)
    {
        if (_searchExpressions is null)
        {
            _searchExpressions = new(DefaultCapacitySearch) { searchExpressions };
            return;
        }

        var index = _searchExpressions.FindIndex(x =>
            x.SearchGroup > searchExpressions.SearchGroup
        );
        if (index == -1)
        {
            _searchExpressions.Add(searchExpressions);
        }
        else
        {
            _searchExpressions.Insert(index, searchExpressions);
        }
    }

    internal void Add(OrderExpression<T> orderExpression)
    {
        _orderExpressions ??= new(DefaultCapacityOrder);
        _orderExpressions.Add(orderExpression);
    }

    internal void Add(IncludeExpression includeExpression)
    {
        _includeExpressions ??= new(DefaultCapacityInclude);
        _includeExpressions.Add(includeExpression);
    }

    internal void Add(string includeString)
    {
        _includeStrings ??= new(DefaultCapacityIncludeString);
        _includeStrings.Add(includeString);
    }
}

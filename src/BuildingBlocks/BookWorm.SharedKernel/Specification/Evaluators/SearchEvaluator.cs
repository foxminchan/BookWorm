using BookWorm.SharedKernel.Specification.Extensions;

namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class SearchEvaluator : IEvaluator
{
    private SearchEvaluator() { }

    public static SearchEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        return specification
            .SearchCriteria.GroupBy(x => x.SearchGroup)
            .Aggregate(query, (current, searchCriteria) => current.Search(searchCriteria));
    }
}

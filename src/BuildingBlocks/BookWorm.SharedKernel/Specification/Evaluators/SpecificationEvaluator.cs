namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class SpecificationEvaluator
{
    private SpecificationEvaluator()
    {
        Evaluators.AddRange(
            [
                WhereEvaluator.Instance,
                SearchEvaluator.Instance,
                IncludeStringEvaluator.Instance,
                IncludeEvaluator.Instance,
                OrderEvaluator.Instance,
                PaginationEvaluator.Instance,
                AsNoTrackingEvaluator.Instance,
                IgnoreQueryFiltersEvaluator.Instance,
                AsSplitQueryEvaluator.Instance,
            ]
        );
    }

    public static SpecificationEvaluator Instance { get; } = new();

    private List<IEvaluator> Evaluators { get; } = [];

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(specification);

        return Evaluators.Aggregate(
            query,
            (current, evaluator) => evaluator.GetQuery(current, specification)
        );
    }
}

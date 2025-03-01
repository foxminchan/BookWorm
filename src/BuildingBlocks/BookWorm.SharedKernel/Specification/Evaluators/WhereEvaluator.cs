namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class WhereEvaluator : IEvaluator
{
    private WhereEvaluator() { }

    public static WhereEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        return specification.Where is null
            ? query
            : specification.Where.Aggregate(query, (current, info) => current.Where(info.Filter));
    }
}

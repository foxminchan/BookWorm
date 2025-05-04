namespace BookWorm.Chassis.Specification.Evaluators;

public sealed class PaginationEvaluator : IEvaluator
{
    private PaginationEvaluator() { }

    public static PaginationEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.Skip > 0)
        {
            query = query.Skip(specification.Skip);
        }

        if (specification.Take >= 0)
        {
            query = query.Take(specification.Take);
        }

        return query;
    }
}

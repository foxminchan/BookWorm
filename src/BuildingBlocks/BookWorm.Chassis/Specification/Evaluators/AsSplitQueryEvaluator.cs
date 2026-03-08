using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chassis.Specification.Evaluators;

internal sealed class AsSplitQueryEvaluator : IEvaluator
{
    private AsSplitQueryEvaluator() { }

    public static AsSplitQueryEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.AsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}

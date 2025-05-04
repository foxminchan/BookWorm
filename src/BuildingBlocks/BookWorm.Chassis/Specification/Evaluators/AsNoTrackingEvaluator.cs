using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chassis.Specification.Evaluators;

public sealed class AsNoTrackingEvaluator : IEvaluator
{
    private AsNoTrackingEvaluator() { }

    public static AsNoTrackingEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }
}

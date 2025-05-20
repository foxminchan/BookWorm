using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chassis.Specification.Evaluators;

public sealed class IncludeStringEvaluator : IEvaluator
{
    private IncludeStringEvaluator() { }

    public static IncludeStringEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        return specification.IncludeStrings.Aggregate(
            query,
            (current, includeString) => current.Include(includeString)
        );
    }
}

using System.Runtime.InteropServices;
using BookWorm.Chassis.Specification.Expressions;
using BookWorm.Chassis.Specification.Extensions;

namespace BookWorm.Chassis.Specification.Evaluators;

public sealed class SearchEvaluator : IEvaluator
{
    private SearchEvaluator() { }

    public static SearchEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        if (specification.SearchExpressions is not List<SearchExpression<T>> { Count: > 0 } list)
        {
            return query;
        }

        if (list.Count == 1)
        {
            return query.ApplySingleLike(list[0]);
        }

        var span = CollectionsMarshal.AsSpan(list);
        return ApplyLike(query, span);
    }

    private static IQueryable<T> ApplyLike<T>(
        IQueryable<T> source,
        ReadOnlySpan<SearchExpression<T>> span
    )
        where T : class
    {
        var groupStart = 0;
        for (var i = 1; i <= span.Length; i++)
        {
            if (i != span.Length && span[i].SearchGroup == span[groupStart].SearchGroup)
            {
                continue;
            }

            source = source.ApplyLikesAsOrGroup(span[groupStart..i]);
            groupStart = i;
        }

        return source;
    }
}

using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using BookWorm.SharedKernel.Specification.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.SharedKernel.Specification.Extensions;

public static class SearchExtensions
{
    private static readonly MethodInfo _likeMethodInfo =
        typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            [typeof(DbFunctions), typeof(string), typeof(string)]
        ) ?? throw new TargetException("The EF.Functions.Like not found!");

    private static readonly MemberExpression _functions = Expression.Property(
        null,
        typeof(Microsoft.EntityFrameworkCore.EF).GetProperty(
            nameof(Microsoft.EntityFrameworkCore.EF.Functions)
        ) ?? throw new TargetException("The EF.Functions not found!")
    );

    public static IQueryable<T> Search<T>(
        this IQueryable<T> source,
        IEnumerable<SearchExpression<T>> criterias
    )
        where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var expr = (
            from criteria in criterias
            where !string.IsNullOrEmpty(criteria.SearchTerm)
            let propertySelector = ParameterReplacerVisitor.Replace(
                criteria.Selector,
                criteria.Selector.Parameters[0],
                parameter
            ) as LambdaExpression
                ?? throw new InvalidExpressionException()
            let searchTermAsExpression = Expression.Constant(criteria.SearchTerm)
            select Expression.Call(
                _likeMethodInfo,
                _functions,
                propertySelector.Body,
                searchTermAsExpression
            )
        ).Aggregate<MethodCallExpression, Expression?>(
            null,
            (current, likeExpression) =>
                current is null ? likeExpression : Expression.OrElse(current, likeExpression)
        );

        return expr is null
            ? source
            : source.Where(Expression.Lambda<Func<T, bool>>(expr, parameter));
    }
}

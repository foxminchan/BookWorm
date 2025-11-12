using System.Linq.Expressions;
using System.Reflection;
using BookWorm.Chassis.Specification.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BookWorm.Chassis.Specification.Extensions;

public static class SearchExtension
{
    private static readonly MethodInfo _likeMethodInfo = typeof(DbFunctionsExtensions).GetMethod(
        nameof(DbFunctionsExtensions.Like),
        [typeof(DbFunctions), typeof(string), typeof(string)]
    )!;

    private static readonly MemberExpression _functions = Expression.Property(
        null,
        typeof(Microsoft.EntityFrameworkCore.EF).GetProperty(
            nameof(Microsoft.EntityFrameworkCore.EF.Functions)
        )!
    );

    private static MemberExpression StringAsExpression(string value)
    {
        return Expression.Property(
            Expression.Constant(new StringVar(value)),
            typeof(StringVar).GetProperty(nameof(StringVar.Format))!
        );
    }

    extension<T>(IQueryable<T> source)
        where T : class
    {
        public IQueryable<T> ApplySingleLike(SearchExpression<T> searchExpression)
        {
            var param = searchExpression.Selector.Parameters[0];
            var selectorExpr = searchExpression.Selector.Body;
            var patternExpr = StringAsExpression(searchExpression.SearchTerm);

            var likeExpr = Expression.Call(
                null,
                _likeMethodInfo,
                _functions,
                selectorExpr,
                patternExpr
            );

            return source.Where(Expression.Lambda<Func<T, bool>>(likeExpr, param));
        }

        public IQueryable<T> ApplyLikesAsOrGroup(
            ReadOnlySpan<SearchExpression<T>> searchExpressions
        )
        {
            Expression? combinedExpr = null;
            ParameterExpression? mainParam = null;
            ParameterReplacerVisitor? visitor = null;

            foreach (var searchExpression in searchExpressions)
            {
                mainParam ??= searchExpression.Selector.Parameters[0];

                var selectorExpr = searchExpression.Selector.Body;
                if (mainParam != searchExpression.Selector.Parameters[0])
                {
                    visitor ??= new(searchExpression.Selector.Parameters[0], mainParam);
                    visitor.Update(searchExpression.Selector.Parameters[0], mainParam);
                    selectorExpr = visitor.Visit(selectorExpr);
                }

                var patternExpr = StringAsExpression(searchExpression.SearchTerm);

                var likeExpr = Expression.Call(
                    null,
                    _likeMethodInfo,
                    _functions,
                    selectorExpr,
                    patternExpr
                );

                combinedExpr = combinedExpr is null
                    ? likeExpr
                    : Expression.OrElse(combinedExpr, likeExpr);
            }

            return combinedExpr is null || mainParam is null
                ? source
                : source.Where(Expression.Lambda<Func<T, bool>>(combinedExpr, mainParam));
        }
    }

    private sealed record StringVar(string Format);
}

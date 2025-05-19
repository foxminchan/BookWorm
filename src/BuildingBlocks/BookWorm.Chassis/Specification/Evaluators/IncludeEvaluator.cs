using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ZLinq;

namespace BookWorm.Chassis.Specification.Evaluators;

public sealed class IncludeEvaluator : IEvaluator
{
    private static readonly MethodInfo _includeMethodInfo =
        typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
            .AsValueEnumerable()
            .Single(mi =>
                mi.IsPublic
                && mi.GetGenericArguments().Length == 2
                && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                    == typeof(IQueryable<>)
                && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                    == typeof(Expression<>)
            );

    private static readonly MethodInfo _thenIncludeAfterReferenceMethodInfo =
        typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
            .AsValueEnumerable()
            .Single(mi =>
                mi.IsPublic
                && mi.GetGenericArguments().Length == 3
                && mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter
                && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                    == typeof(IIncludableQueryable<,>)
                && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                    == typeof(Expression<>)
            );

    private static readonly MethodInfo _thenIncludeAfterEnumerableMethodInfo =
        typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
            .AsValueEnumerable()
            .Single(mi =>
                mi.IsPublic
                && mi.GetGenericArguments().Length == 3
                && !mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter
                && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                    == typeof(IIncludableQueryable<,>)
                && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                    == typeof(Expression<>)
            );

    private static readonly ConcurrentDictionary<
        CacheKey,
        Func<IQueryable, LambdaExpression, IQueryable>
    > _cache = new();

    public static readonly IncludeEvaluator Instance = new();

    private IncludeEvaluator() { }

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        foreach (var includeExpression in specification.IncludeExpressions)
        {
            var lambdaExpr = includeExpression.LambdaExpression;

            switch (includeExpression.Type)
            {
                case IncludeType.Include:
                {
                    var key = new CacheKey(typeof(T), lambdaExpr.ReturnType, null);
                    var include = _cache.GetOrAdd(key, CreateIncludeDelegate);
                    query = (IQueryable<T>)include(query, lambdaExpr);
                    break;
                }
                case IncludeType.ThenInclude:
                {
                    var key = new CacheKey(
                        typeof(T),
                        lambdaExpr.ReturnType,
                        includeExpression.PreviousPropertyType
                    );
                    var include = _cache.GetOrAdd(key, CreateThenIncludeDelegate);
                    query = (IQueryable<T>)include(query, lambdaExpr);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(includeExpression.Type),
                        "Invalid include type."
                    );
            }
        }

        return query;
    }

    private static Func<IQueryable, LambdaExpression, IQueryable> CreateIncludeDelegate(
        CacheKey cacheKey
    )
    {
        var includeMethod = _includeMethodInfo.MakeGenericMethod(
            cacheKey.EntityType,
            cacheKey.PropertyType
        );
        var sourceParameter = Expression.Parameter(typeof(IQueryable));
        var selectorParameter = Expression.Parameter(typeof(LambdaExpression));

        var call = Expression.Call(
            includeMethod,
            Expression.Convert(
                sourceParameter,
                typeof(IQueryable<>).MakeGenericType(cacheKey.EntityType)
            ),
            Expression.Convert(
                selectorParameter,
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(cacheKey.EntityType, cacheKey.PropertyType)
                )
            )
        );

        var lambda = Expression.Lambda<Func<IQueryable, LambdaExpression, IQueryable>>(
            call,
            sourceParameter,
            selectorParameter
        );
        return lambda.Compile();
    }

    private static Func<IQueryable, LambdaExpression, IQueryable> CreateThenIncludeDelegate(
        CacheKey cacheKey
    )
    {
        Debug.Assert(cacheKey.PreviousReturnType is not null);

        var thenIncludeInfo = IsGenericEnumerable(
            cacheKey.PreviousReturnType,
            out var previousPropertyType
        )
            ? _thenIncludeAfterEnumerableMethodInfo
            : _thenIncludeAfterReferenceMethodInfo;

        var thenIncludeMethod = thenIncludeInfo.MakeGenericMethod(
            cacheKey.EntityType,
            previousPropertyType,
            cacheKey.PropertyType
        );
        var sourceParameter = Expression.Parameter(typeof(IQueryable));
        var selectorParameter = Expression.Parameter(typeof(LambdaExpression));

        var call = Expression.Call(
            thenIncludeMethod,
            Expression.Convert(
                sourceParameter,
                typeof(IIncludableQueryable<,>).MakeGenericType(
                    cacheKey.EntityType,
                    cacheKey.PreviousReturnType
                )
            ),
            Expression.Convert(
                selectorParameter,
                typeof(Expression<>).MakeGenericType(
                    typeof(Func<,>).MakeGenericType(previousPropertyType, cacheKey.PropertyType)
                )
            )
        );

        var lambda = Expression.Lambda<Func<IQueryable, LambdaExpression, IQueryable>>(
            call,
            sourceParameter,
            selectorParameter
        );
        return lambda.Compile();
    }

    private static bool IsGenericEnumerable(Type type, out Type propertyType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            propertyType = type.GenericTypeArguments[0];
            return true;
        }

        propertyType = type;
        return false;
    }

    private readonly record struct CacheKey(
        Type EntityType,
        Type PropertyType,
        Type? PreviousReturnType
    );
}

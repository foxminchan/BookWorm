using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using IncludeExpression = BookWorm.SharedKernel.Specification.Expressions.IncludeExpression;

namespace BookWorm.SharedKernel.Specification.Evaluators;

public sealed class IncludeEvaluator : IEvaluator
{
    private static readonly MethodInfo _includeMethodInfo =
        typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
            .Single(mi =>
                mi.GetGenericArguments().Length == 2
                && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                    == typeof(IQueryable<>)
                && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                    == typeof(Expression<>)
            );

    private static readonly MethodInfo _thenIncludeAfterReferenceMethodInfo =
        typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo()
            .GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
            .Single(mi =>
                mi.GetGenericArguments().Length == 3
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
            .Where(mi => mi.GetGenericArguments().Length == 3)
            .Single(mi =>
            {
                var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1];

                return typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && mi.GetParameters()[0].ParameterType.GetGenericTypeDefinition()
                        == typeof(IIncludableQueryable<,>)
                    && mi.GetParameters()[1].ParameterType.GetGenericTypeDefinition()
                        == typeof(Expression<>);
            });

    private IncludeEvaluator() { }

    public static IncludeEvaluator Instance { get; } = new();

    public IQueryable<T> GetQuery<T>(IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        query = specification.IncludeStrings.Aggregate(
            query,
            (current, includeString) => current.Include(includeString)
        );

        return specification.IncludesExpression.Aggregate(
            query,
            (current, includeInfo) =>
                includeInfo.Type switch
                {
                    IncludeType.Include => BuildInclude(current, includeInfo),
                    IncludeType.ThenInclude => BuildThenInclude(current, includeInfo),
                    _ => current,
                }
        );
    }

    private static IQueryable<T> BuildInclude<T>(IQueryable<T> query, IncludeExpression includeInfo)
        where T : class
    {
        var result = _includeMethodInfo
            .MakeGenericMethod(includeInfo.EntityType, includeInfo.PropertyType)
            .Invoke(null, [query, includeInfo.LambdaExpression]);

        _ = result ?? throw new TargetException();

        return (IQueryable<T>)result;
    }

    private static IQueryable<T> BuildThenInclude<T>(
        IQueryable<T> query,
        IncludeExpression includeInfo
    )
        where T : class
    {
        var result = (
            IsGenericEnumerable(includeInfo.PreviousPropertyType!, out var previousPropertyType)
                ? _thenIncludeAfterEnumerableMethodInfo
                : _thenIncludeAfterReferenceMethodInfo
        )
            .MakeGenericMethod(
                includeInfo.EntityType,
                previousPropertyType,
                includeInfo.PropertyType
            )
            .Invoke(null, [query, includeInfo.LambdaExpression]);

        _ = result ?? throw new TargetException();

        return (IQueryable<T>)result;
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
}

﻿using System.Linq.Expressions;
using BookWorm.SharedKernel.Specification.Expressions;

namespace BookWorm.SharedKernel.Specification.Builders;

public static partial class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T> Include<T>(
        this ISpecificationBuilder<T> builder,
        string includeString
    )
        where T : class
    {
        builder.Specification.Add(includeString);
        return builder;
    }

    public static IIncludeSpecificationBuilder<T, TProperty> Include<T, TProperty>(
        this ISpecificationBuilder<T> builder,
        Expression<Func<T, TProperty>> navigationSelector
    )
        where T : class
    {
        var expr = new IncludeExpression(navigationSelector, IncludeType.Include);
        builder.Specification.Add(expr);

        var includeBuilder = new IncludeSpecificationBuilder<T, TProperty>(builder.Specification);
        return includeBuilder;
    }

    public static IIncludeSpecificationBuilder<TEntity, TProperty> ThenInclude<
        TEntity,
        TPreviousProperty,
        TProperty
    >(
        this IIncludeSpecificationBuilder<TEntity, IEnumerable<TPreviousProperty>> builder,
        Expression<Func<TPreviousProperty, TProperty>> navigationSelector
    )
        where TEntity : class
    {
        var expr = new IncludeExpression(navigationSelector, IncludeType.ThenInclude);
        builder.Specification.Add(expr);

        var includeBuilder = new IncludeSpecificationBuilder<TEntity, TProperty>(
            builder.Specification
        );
        return includeBuilder;
    }
}

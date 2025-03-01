using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public class IncludeExpression
{
    private IncludeExpression(
        LambdaExpression expression,
        Type entityType,
        Type propertyType,
        Type? previousPropertyType,
        IncludeType includeType
    )
    {
        LambdaExpression = expression;
        EntityType = entityType;
        PropertyType = propertyType;
        PreviousPropertyType = previousPropertyType;
        Type = includeType;
    }

    public IncludeExpression(LambdaExpression expression, Type entityType, Type propertyType)
        : this(expression, entityType, propertyType, null, IncludeType.Include) { }

    public IncludeExpression(
        LambdaExpression expression,
        Type entityType,
        Type propertyType,
        Type previousPropertyType
    )
        : this(expression, entityType, propertyType, previousPropertyType, IncludeType.ThenInclude)
    { }

    public LambdaExpression LambdaExpression { get; }

    public Type EntityType { get; }

    public Type PropertyType { get; }

    public Type? PreviousPropertyType { get; }

    public IncludeType Type { get; }
}

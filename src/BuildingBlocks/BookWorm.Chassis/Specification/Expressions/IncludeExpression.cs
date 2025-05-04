using System.Linq.Expressions;

namespace BookWorm.Chassis.Specification.Expressions;

public class IncludeExpression
{
    public IncludeExpression(LambdaExpression expression)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));

        LambdaExpression = expression;
        PreviousPropertyType = null;
        Type = IncludeType.Include;
    }

    public IncludeExpression(LambdaExpression expression, Type previousPropertyType)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        _ = previousPropertyType ?? throw new ArgumentNullException(nameof(previousPropertyType));

        LambdaExpression = expression;
        PreviousPropertyType = previousPropertyType;
        Type = IncludeType.ThenInclude;
    }

    public LambdaExpression LambdaExpression { get; }

    public Type? PreviousPropertyType { get; }

    public IncludeType Type { get; }
}

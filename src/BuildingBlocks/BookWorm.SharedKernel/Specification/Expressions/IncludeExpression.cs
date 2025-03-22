using System.Linq.Expressions;

namespace BookWorm.SharedKernel.Specification.Expressions;

public class IncludeExpression
{
    public LambdaExpression LambdaExpression { get; }

    public IncludeType Type { get; }

    public IncludeExpression(LambdaExpression expression, IncludeType includeType)
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));

        LambdaExpression = expression;
        Type = includeType;
    }
}

using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using BookWorm.Chassis.AI.Decisions.Abstractions;

namespace BookWorm.Chassis.AI.Decisions.Interoperability;

public sealed class DecisionResponse<TResult>
{
    private readonly IReadOnlyDictionary<string, object> _distributions;
    private readonly JsonSerializerOptions _serializerOptions;

    internal DecisionResponse(
        TResult result,
        DecisionResponse response,
        IReadOnlyDictionary<string, object> distributions,
        JsonSerializerOptions serializerOptions
    )
    {
        Result = result;
        Response = response;
        _distributions = distributions;
        _serializerOptions = serializerOptions;
    }

    public TResult Result { get; }

    public DecisionResponse Response { get; }

    public IReadOnlyDictionary<TEnum, double> GetDistribution<TEnum>(
        Expression<Func<TResult, TEnum>> selector
    )
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(selector);

        var typeInfo = (JsonTypeInfo<TResult>)_serializerOptions.GetTypeInfo(typeof(TResult));
        var questionId = DecisionDefinition<TResult>.ResolvePropertyJsonName(
            selector,
            typeInfo,
            _serializerOptions
        );

        if (
            _distributions.TryGetValue(questionId, out var value)
            && value is IReadOnlyDictionary<TEnum, double> distribution
        )
        {
            return distribution;
        }

        throw new InvalidOperationException(
            $"No enum probability distribution was recorded for property '{questionId}'."
        );
    }
}

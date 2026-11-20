using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.AI;

namespace BookWorm.Chassis.AI.Decisions;

public sealed partial class DecisionDefinition<TResult>
{
    private readonly IReadOnlyList<BoundQuestion> _bindings;

    private DecisionDefinition(
        IReadOnlyList<BoundQuestion> bindings,
        JsonSerializerOptions serializerOptions,
        JsonTypeInfo<TResult> resultTypeInfo
    )
    {
        _bindings = bindings;
        Questions = bindings.Select(static b => b.Question).ToList();
        SerializerOptions = serializerOptions;
        ResultTypeInfo = resultTypeInfo;
    }

    public IReadOnlyList<DecisionQuestion> Questions { get; }

    public JsonTypeInfo<TResult> ResultTypeInfo { get; }

    public JsonSerializerOptions SerializerOptions { get; }

    public static DecisionDefinition<TResult> Create(
        Action<Builder> configure,
        JsonSerializerOptions? serializerOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = serializerOptions ?? AIJsonUtilities.DefaultOptions;
        var resultTypeInfo = (JsonTypeInfo<TResult>)options.GetTypeInfo(typeof(TResult));

        var builder = new Builder(options, resultTypeInfo);
        configure(builder);

        if (builder.Bindings.Count == 0)
        {
            throw new InvalidOperationException(
                "A DecisionDefinition must declare at least one bound question."
            );
        }

        return new DecisionDefinition<TResult>(builder.Bindings, options, resultTypeInfo);
    }

    public AIFunction AsAIFunction<TState>(
        IDecisionClient client,
        AIFunctionFactoryOptions? functionOptions = null,
        DecisionOptions? decisionOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(client);

        var clonedOptions = decisionOptions?.Clone();
        var options = functionOptions ?? new AIFunctionFactoryOptions();
        if (options.SerializerOptions is null)
        {
            options = new AIFunctionFactoryOptions
            {
                Name = options.Name,
                Description = options.Description,
                SerializerOptions = SerializerOptions,
                JsonSchemaCreateOptions = options.JsonSchemaCreateOptions,
                AdditionalProperties = options.AdditionalProperties,
                ConfigureParameterBinding = options.ConfigureParameterBinding,
                MarshalResult = options.MarshalResult,
            };
        }

        return AIFunctionFactory.Create(
            async (TState state, CancellationToken cancellationToken) =>
            {
                var response = await client
                    .GetDecisionAsync(
                        state,
                        this,
                        options: clonedOptions?.Clone(),
                        cancellationToken: cancellationToken
                    )
                    .ConfigureAwait(false);
                return response.Result;
            },
            options
        );
    }

    internal DecisionResponse<TResult> BindResponse(DecisionResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var unansweredIds = new List<string>();
        var resolvedAnswers = new Dictionary<string, DecisionAnswer>(
            _bindings.Count,
            StringComparer.Ordinal
        );

        foreach (var binding in _bindings)
        {
            var answer = response.GetAnswer(binding.Question.Id);
            if (answer is UnansweredDecisionAnswer)
            {
                unansweredIds.Add(binding.Question.Id);
                continue;
            }

            if (answer is null)
            {
                throw new InvalidOperationException(
                    $"No answer was returned for bound decision question '{binding.Question.Id}'."
                );
            }

            if (answer.Kind != binding.Question.Kind)
            {
                throw new InvalidOperationException(
                    $"Decision question '{binding.Question.Id}' expected answer kind '{binding.Question.Kind}' but received '{answer.Kind}'."
                );
            }

            resolvedAnswers[binding.Question.Id] = answer;
        }

        if (unansweredIds.Count > 0)
        {
            throw new DecisionUnansweredException(response, unansweredIds);
        }

        var jsonObject = new JsonObject();
        var distributions = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var binding in _bindings)
        {
            var answer = resolvedAnswers[binding.Question.Id];
            binding.Apply(answer, jsonObject, distributions);
        }

        var result = JsonSerializer.Deserialize(jsonObject, ResultTypeInfo)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize decision response into '{typeof(TResult).Name}'."
            );

        return new DecisionResponse<TResult>(result, response, distributions, SerializerOptions);
    }

    internal static string ResolvePropertyJsonName(
        LambdaExpression selector,
        JsonTypeInfo<TResult> resultTypeInfo,
        JsonSerializerOptions serializerOptions
    )
    {
        var member = ExtractMember(selector);
        var jsonProp = resultTypeInfo.Properties.FirstOrDefault(p =>
            p.AttributeProvider is MemberInfo m
            && string.Equals(m.Name, member.Name, StringComparison.Ordinal)
        );

        if (jsonProp is not null)
        {
            return jsonProp.Name;
        }

        var jsonPropertyName = member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
        if (!string.IsNullOrWhiteSpace(jsonPropertyName))
        {
            return jsonPropertyName;
        }

        return serializerOptions.PropertyNamingPolicy?.ConvertName(member.Name) ?? member.Name;
    }

    private static MemberInfo ExtractMember(LambdaExpression selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        var body = selector.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            body = unary.Operand;
        }

        if (body is MemberExpression memberExpression)
        {
            return memberExpression.Member;
        }

        throw new ArgumentException(
            "Selector must be a simple property access expression.",
            nameof(selector)
        );
    }

    internal sealed record BoundQuestion(
        DecisionQuestion Question,
        Action<DecisionAnswer, JsonObject, Dictionary<string, object>> Apply
    );
}

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Chassis.AI.Decisions.Abstractions;

namespace BookWorm.Chassis.AI.Decisions.Interoperability;

public sealed record DecisionEnumValue<TEnum>(TEnum Value, string Id, string Description)
    where TEnum : struct, Enum;

public sealed class DecisionEnumDefinition<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum
>
    where TEnum : struct, Enum
{
    private readonly Dictionary<string, TEnum> _byId;
    private readonly Dictionary<TEnum, string> _toId;

    public DecisionEnumDefinition(JsonSerializerOptions? serializerOptions = null)
    {
        var namingPolicy = ResolveEnumNamingPolicy(serializerOptions);
        var fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static);
        var values = new List<DecisionEnumValue<TEnum>>(fields.Length);
        var candidates = new List<DecisionCandidate>(fields.Length);
        _byId = new(fields.Length, StringComparer.Ordinal);
        _toId = [with(fields.Length)];

        foreach (var field in fields)
        {
            var value = (TEnum)field.GetValue(null)!;
            var id = ResolveMemberId(field, namingPolicy);
            var description = field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? id;

            values.Add(new(value, id, description));
            candidates.Add(new(id, description));
            _byId[id] = value;
            _toId[value] = id;
        }

        Values = values;
        Candidates = candidates;
    }

    public IReadOnlyList<DecisionEnumValue<TEnum>> Values { get; }

    public IReadOnlyList<DecisionCandidate> Candidates { get; }

    public ChoiceDecisionQuestion CreateQuestion(string id, string instructions)
    {
        return new(id, instructions, Candidates);
    }

    public TEnum Parse(string candidateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateId);
        if (_byId.TryGetValue(candidateId, out var value))
        {
            return value;
        }

        throw new InvalidOperationException(
            $"Candidate ID '{candidateId}' is not a valid member of '{typeof(TEnum).Name}'."
        );
    }

    internal bool TryParse(string candidateId, out TEnum value)
    {
        return _byId.TryGetValue(candidateId, out value);
    }

    internal string GetId(TEnum value)
    {
        return _toId[value];
    }

    private static string ResolveMemberId(FieldInfo field, JsonNamingPolicy? namingPolicy)
    {
        var jsonStringName = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
        if (!string.IsNullOrWhiteSpace(jsonStringName))
        {
            return jsonStringName;
        }

        var enumMemberName = field.GetCustomAttribute<EnumMemberAttribute>()?.Value;
        if (!string.IsNullOrWhiteSpace(enumMemberName))
        {
            return enumMemberName;
        }

        var jsonPropertyName = field.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
        if (!string.IsNullOrWhiteSpace(jsonPropertyName))
        {
            return jsonPropertyName;
        }

        return namingPolicy?.ConvertName(field.Name) ?? field.Name;
    }

    private static JsonNamingPolicy? ResolveEnumNamingPolicy(
        JsonSerializerOptions? serializerOptions
    )
    {
        return serializerOptions?.PropertyNamingPolicy;
    }
}

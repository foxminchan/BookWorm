using System.Text.Json;
using Match = PactNet.Matchers.Match;

namespace BookWorm.Testing.Pact.Messaging;

internal static class PactTypeMatcher
{
    public static object? Create(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element
                .EnumerateObject()
                .ToDictionary(property => property.Name, property => Create(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(Create).ToArray(),
            JsonValueKind.String => Match.Type(element.GetString()!),
            JsonValueKind.Number => CreateNumberMatcher(element),
            JsonValueKind.True => Match.Type(true),
            JsonValueKind.False => Match.Type(false),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => throw new InvalidOperationException(
                "Undefined JSON token in Pact message is not supported."
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported JSON token in Pact message: {element.ValueKind}."
            ),
        };
    }

    private static object CreateNumberMatcher(JsonElement element)
    {
        return element.TryGetInt64(out var integer)
            ? Match.Type(integer)
            : Match.Type(element.GetDecimal());
    }
}

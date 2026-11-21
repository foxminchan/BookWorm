using System.Text.Json;
using BookWorm.Chat.Infrastructure.Decisions;

namespace BookWorm.Chat.Orchestration.Executors.Disambiguation;

internal sealed record RouteDisambiguationOption(string Id, string Label, string Description);

internal sealed record RouteDisambiguationRequest(
    string CallId,
    string Question,
    RouteTarget SuggestedTarget,
    double Confidence,
    IReadOnlyList<RouteDisambiguationOption> Options,
    IReadOnlyList<ChatMessage> Messages
) : IExternalRequestEnvelope
{
    public AIContent GetInnerRequestContent()
    {
        Dictionary<string, object?> arguments = new(StringComparer.Ordinal)
        {
            ["question"] = Question,
            ["suggestedRoute"] = RouteDisambiguationConstants.ToAgentId(SuggestedTarget),
            ["confidence"] = Confidence,
            ["options"] = Options,
        };

        return new FunctionCallContent(CallId, RouteDisambiguationConstants.ToolName, arguments);
    }

    public object CreateResponse(IList<ChatMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var selectedTarget = SuggestedTarget;

        messageLoop:
        foreach (var message in messages)
        {
            foreach (var content in message.Contents)
            {
                if (content is not FunctionResultContent resultContent)
                {
                    continue;
                }

                var raw = ExtractRawResult(resultContent.Result);
                if (RouteDisambiguationConstants.TryParseTarget(raw, out var parsedFromTool))
                {
                    selectedTarget = parsedFromTool;
                    break messageLoop;
                }
            }

            if (RouteDisambiguationConstants.TryParseTarget(message.Text, out var parsedFromText))
            {
                selectedTarget = parsedFromText;
                break messageLoop;
            }
        }

        return new RouteDisambiguationResponse(selectedTarget, Messages);
    }

    private static string? ExtractRawResult(object? result)
    {
        return result switch
        {
            null => null,
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } json => json.GetString(),
            JsonElement { ValueKind: JsonValueKind.Object } json
                when json.TryGetProperty("route", out var routeProp)
                    && routeProp.ValueKind == JsonValueKind.String => routeProp.GetString(),
            _ => result.ToString(),
        };
    }
}

internal sealed record RouteDisambiguationResponse(
    RouteTarget SelectedTarget,
    IReadOnlyList<ChatMessage> Messages
);

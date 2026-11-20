using System.Text.Json;

namespace BookWorm.Chat.Orchestration;

internal sealed class AGUIRunState
{
    public JsonElement? State { get; init; }

    public JsonElement? Context { get; init; }

    public IReadOnlyList<AITool> ClientTools { get; init; } = [];
}

internal static class AGUIRunContext
{
    private const string StateKey = "ag_ui_state";
    private const string ContextKey = "ag_ui_context";

    private static readonly AsyncLocal<AGUIRunState?> _currentState = new();

    public static AGUIRunState? Current => _currentState.Value;

    public static IDisposable Push(AgentRunOptions? options)
    {
        var previous = _currentState.Value;

        if (options is not ChatClientAgentRunOptions { ChatOptions: { } chatOptions })
        {
            return new Scope(previous);
        }

        JsonElement? state = null;
        JsonElement? context = null;

        if (chatOptions.AdditionalProperties is { } props)
        {
            if (props.TryGetValue(StateKey, out var stateObj) && stateObj is JsonElement s)
            {
                state = s;
            }

            if (props.TryGetValue(ContextKey, out var contextObj) && contextObj is JsonElement c)
            {
                context = c;
            }
        }

        IReadOnlyList<AITool> clientTools = chatOptions.Tools is { Count: > 0 }
            ? [.. chatOptions.Tools]
            : [];

        _currentState.Value = new()
        {
            State = state,
            Context = context,
            ClientTools = clientTools,
        };

        return new Scope(previous);
    }

    private sealed class Scope(AGUIRunState? previous) : IDisposable
    {
        public void Dispose()
        {
            _currentState.Value = previous;
        }
    }
}

using System.Runtime.CompilerServices;
using BookWorm.Chat.Orchestration.Executors.Disambiguation;

namespace BookWorm.Chat.Orchestration;

internal sealed class AGUIWorkflowAgent(AIAgent innerAgent) : DelegatingAIAgent(innerAgent)
{
    protected override async Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        using var _ = AGUIRunContext.Push(options);
        var normalizedMessages = NormalizeDisambiguationResume(messages);
        return await base.RunCoreAsync(normalizedMessages, session, options, cancellationToken)
            .ConfigureAwait(false);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using var _ = AGUIRunContext.Push(options);
        var normalizedMessages = NormalizeDisambiguationResume(messages);

        await foreach (
            var update in base.RunCoreStreamingAsync(
                    normalizedMessages,
                    session,
                    options,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
        {
            yield return update;
        }
    }

    /// <summary>
    ///     Converts a plain-text user reply into a <see cref="FunctionResultContent" /> when the
    ///     conversation has an unanswered <c>disambiguateRoute</c> interrupt, allowing
    ///     <see cref="WorkflowSession" /> to resume the paused <see cref="RequestPort" /> even for
    ///     clients that reply with text instead of a tool result.
    /// </summary>
    internal static IReadOnlyList<ChatMessage> NormalizeDisambiguationResume(
        IEnumerable<ChatMessage> messages
    )
    {
        List<ChatMessage> list = [.. messages];
        if (list.Count < 2)
        {
            return list;
        }

        var lastMessage = list[^1];
        if (lastMessage.Role != ChatRole.User || string.IsNullOrWhiteSpace(lastMessage.Text))
        {
            return list;
        }

        var pendingCall = FindUnansweredDisambiguationCall(list);
        if (
            pendingCall is null
            || !RouteDisambiguationConstants.TryParseTarget(
                lastMessage.Text,
                out var selectedTarget
            )
        )
        {
            return list;
        }

        var toolResult = new FunctionResultContent(
            pendingCall.CallId,
            RouteDisambiguationConstants.ToAgentId(selectedTarget)
        );
        list[^1] = new(ChatRole.Tool, [toolResult]);
        return list;
    }

    private static FunctionCallContent? FindUnansweredDisambiguationCall(List<ChatMessage> messages)
    {
        HashSet<string> answeredCallIds = [with(StringComparer.Ordinal)];
        FunctionCallContent? latestPendingCall = null;

        // Walk backward before the trailing user message to find the most recent unanswered call
        messageLoop:
        for (var i = messages.Count - 2; i >= 0; i--)
        {
            foreach (var content in messages[i].Contents)
            {
                switch (content)
                {
                    case FunctionResultContent result:
                        answeredCallIds.Add(result.CallId);
                        break;
                    case FunctionCallContent call
                        when string.Equals(
                            call.Name,
                            RouteDisambiguationConstants.ToolName,
                            StringComparison.Ordinal
                        ) && !answeredCallIds.Contains(call.CallId):
                        latestPendingCall = call;
                        break messageLoop;
                }
            }
        }

        return latestPendingCall;
    }
}

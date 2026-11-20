using System.Runtime.CompilerServices;

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
        return await base.RunCoreAsync(messages, session, options, cancellationToken)
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

        await foreach (
            var update in base.RunCoreStreamingAsync(messages, session, options, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            yield return update;
        }
    }
}

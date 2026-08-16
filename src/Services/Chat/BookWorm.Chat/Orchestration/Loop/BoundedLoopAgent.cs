using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;

namespace BookWorm.Chat.Orchestration.Loop;

internal sealed class BoundedLoopAgent : DelegatingAIAgent
{
    public BoundedLoopAgent(AIAgent innerAgent, TimeSpan executionTimeout)
        : base(innerAgent)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(executionTimeout, TimeSpan.Zero);

        ExecutionTimeout = executionTimeout;
    }

    private TimeSpan ExecutionTimeout { get; }

    protected override async Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        using var timeoutSource = CreateTimeoutSource(cancellationToken);

        return await InnerAgent
            .RunAsync(messages, session, options, timeoutSource.Token)
            .ConfigureAwait(false);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        using var timeoutSource = CreateTimeoutSource(cancellationToken);

        await foreach (
            var update in InnerAgent
                .RunStreamingAsync(messages, session, options, timeoutSource.Token)
                .ConfigureAwait(false)
        )
        {
            yield return update;
        }
    }

    private CancellationTokenSource CreateTimeoutSource(CancellationToken cancellationToken)
    {
        var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(ExecutionTimeout);
        return timeoutSource;
    }
}

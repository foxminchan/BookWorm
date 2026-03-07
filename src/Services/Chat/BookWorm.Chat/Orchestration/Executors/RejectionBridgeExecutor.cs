using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Orchestration.Executors;

/// <summary>
/// Bridges a rejected <see cref="ChatMessage"/> (role = Assistant) into a
/// <see cref="List{ChatMessage}"/> so the existing <see cref="ResponseFormatterExecutor"/>
/// can format it without reaching the agent handoff layer.
/// </summary>
internal sealed class RejectionBridgeExecutor()
    : Executor<ChatMessage, List<ChatMessage>>("RejectionBridgeExecutor")
{
    public override ValueTask<List<ChatMessage>> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        return ValueTask.FromResult<List<ChatMessage>>([message]);
    }
}

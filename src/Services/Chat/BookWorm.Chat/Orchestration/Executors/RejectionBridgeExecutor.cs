using Microsoft.Agents.AI.Workflows;

namespace BookWorm.Chat.Orchestration.Executors;

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

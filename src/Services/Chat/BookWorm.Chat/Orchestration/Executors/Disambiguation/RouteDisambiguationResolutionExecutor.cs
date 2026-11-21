using BookWorm.Chat.Infrastructure.Decisions;

namespace BookWorm.Chat.Orchestration.Executors.Disambiguation;

internal sealed class RouteDisambiguationResolutionExecutor()
    : Executor<RouteDisambiguationResponse, RoutingDecision>(
        nameof(RouteDisambiguationResolutionExecutor)
    )
{
    public override ValueTask<RoutingDecision> HandleAsync(
        RouteDisambiguationResponse message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        var lastMessage = message.Messages[^1];
        return ValueTask.FromResult(
            new RoutingDecision(
                message.SelectedTarget,
                lastMessage,
                Confidence: 1.0,
                Messages: message.Messages
            )
        );
    }
}

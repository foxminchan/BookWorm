using BookWorm.Chat.Infrastructure.Decisions;

namespace BookWorm.Chat.Orchestration.Executors.Disambiguation;

internal sealed class RouteDisambiguationPrepExecutor()
    : Executor<RoutingDecision, RouteDisambiguationRequest>(nameof(RouteDisambiguationPrepExecutor))
{
    public override ValueTask<RouteDisambiguationRequest> HandleAsync(
        RoutingDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        return ValueTask.FromResult(
            new RouteDisambiguationRequest(
                $"disambiguate_{Guid.CreateVersion7():N}",
                "I want to make sure the right specialist helps you. Which area best fits your request?",
                message.Target,
                message.Confidence,
                RouteDisambiguationConstants.DefaultOptions,
                message.EffectiveMessages
            )
        );
    }
}

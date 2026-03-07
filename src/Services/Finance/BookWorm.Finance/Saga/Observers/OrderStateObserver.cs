using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BookWorm.Finance.Saga.Observers;

public sealed class OrderStateObserver : IStateObserver<OrderState>
{
    private static readonly ActivitySource _activitySource = new(nameof(OrderStateObserver));
    private static readonly Meter _meter = new(nameof(OrderStateObserver));

    private static readonly Counter<long> _stateTransitionCounter = _meter.CreateCounter<long>(
        "finance.saga.state_transitions",
        description: "Number of order saga state transitions"
    );

    public Task StateChanged(
        BehaviorContext<OrderState> context,
        State currentState,
        State previousState
    )
    {
        _stateTransitionCounter.Add(
            1,
            new("from_state", previousState.Name),
            new("to_state", currentState.Name)
        );

        using var activity = _activitySource.StartActivity();
        activity?.SetTag("saga.order_id", context.Saga.OrderId);
        activity?.SetTag("saga.from_state", previousState.Name);
        activity?.SetTag("saga.to_state", currentState.Name);

        return Task.CompletedTask;
    }
}

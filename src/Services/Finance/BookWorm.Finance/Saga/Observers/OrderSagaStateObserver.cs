using System.Diagnostics.Metrics;

namespace BookWorm.Finance.Saga.Observers;

/// <summary>
///     Emits the <c>finance.saga.state_transitions</c> counter that was previously
///     tracked by the MassTransit <c>OrderStateObserver</c>.  Call
///     <see cref="RecordTransition" /> from each saga handler after updating
///     <see cref="OrderSaga.CurrentState" />.
/// </summary>
internal sealed class OrderSagaStateObserver
{
    private static readonly Meter _meter = new("BookWorm.Finance", "1.0.0");

    private static readonly Counter<long> _stateTransitions = _meter.CreateCounter<long>(
        "finance.saga.state_transitions",
        description: "Number of order saga state transitions."
    );

    /// <summary>
    ///     Increments the <c>finance.saga.state_transitions</c> counter.
    /// </summary>
    /// <param name="orderId">The saga correlation key.</param>
    /// <param name="previousState">The state the saga was in before the transition.</param>
    /// <param name="currentState">The state the saga transitioned into.</param>
    public void RecordTransition(Guid orderId, string previousState, string currentState)
    {
        _stateTransitions.Add(
            1,
            new KeyValuePair<string, object?>("saga.order_id", orderId),
            new KeyValuePair<string, object?>("saga.previous_state", previousState),
            new KeyValuePair<string, object?>("saga.current_state", currentState)
        );
    }
}

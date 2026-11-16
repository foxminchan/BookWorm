namespace BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;

public enum OrderLifecycleStage : byte
{
    [Description("Not Created")]
    NotCreated = 0,

    [Description("Pending")]
    Pending = 1,

    [Description("Completed")]
    Completed = 2,

    [Description("Cancelled")]
    Cancelled = 3,
}

public sealed record OrderLifecycleState(
    OrderLifecycleStage Stage,
    OrderId? OrderId = null,
    BuyerId? BuyerId = null,
    DateTimeOffset? TransitionedAt = null
);

public enum OrderLifecycleAction : byte
{
    [Description("Place Order")]
    Place = 0,

    [Description("Complete Order")]
    Complete = 1,

    [Description("Cancel Order")]
    Cancel = 2,
}

public sealed record OrderLifecycleCommand(
    OrderLifecycleAction Action,
    DateTimeOffset Now,
    OrderId? OrderId = null,
    BuyerId? BuyerId = null
);

public sealed record OrderTransition(
    OrderLifecycleStage TargetStage,
    DateTimeOffset OccurredAt,
    OrderId? OrderId = null,
    BuyerId? BuyerId = null
);

public static class OrderLifecycleDecider
{
    public static readonly Decider<
        OrderLifecycleState,
        OrderTransition
    >.CommandDecider<OrderLifecycleCommand> Instance = new Decider<
        OrderLifecycleState,
        OrderTransition
    >(Evolve, () => new(OrderLifecycleStage.NotCreated)).For<OrderLifecycleCommand>(Decide);

    private static OrderTransition[] Decide(
        OrderLifecycleCommand command,
        OrderLifecycleState state
    )
    {
        return (command.Action, state.Stage) switch
        {
            (OrderLifecycleAction.Place, OrderLifecycleStage.NotCreated) =>
            [
                new(
                    OrderLifecycleStage.Pending,
                    command.Now,
                    command.OrderId ?? throw new OrderingDomainException("OrderId is required."),
                    command.BuyerId ?? throw new OrderingDomainException("BuyerId is required.")
                ),
            ],
            (OrderLifecycleAction.Complete, OrderLifecycleStage.Pending) =>
            [
                new(OrderLifecycleStage.Completed, command.Now, state.OrderId),
            ],
            (OrderLifecycleAction.Complete, OrderLifecycleStage.Completed) => [],
            (OrderLifecycleAction.Cancel, OrderLifecycleStage.Pending) =>
            [
                new(OrderLifecycleStage.Cancelled, command.Now, state.OrderId),
            ],
            (OrderLifecycleAction.Cancel, OrderLifecycleStage.Cancelled) => [],
            (OrderLifecycleAction.Complete, OrderLifecycleStage.Cancelled) =>
                throw new OrderingDomainException(
                    $"Cannot complete order in '{Status.Cancelled}' status; only '{Status.New}' orders can be completed."
                ),
            (OrderLifecycleAction.Cancel, OrderLifecycleStage.Completed) =>
                throw new OrderingDomainException(
                    $"Cannot cancel order in '{Status.Completed}' status; only '{Status.New}' orders can be cancelled."
                ),
            _ => throw new OrderingDomainException(
                $"Invalid action '{command.Action}' for stage '{state.Stage}'."
            ),
        };
    }

    private static OrderLifecycleState Evolve(OrderLifecycleState state, OrderTransition transition)
    {
        return (transition.TargetStage, state.Stage) switch
        {
            (OrderLifecycleStage.Pending, OrderLifecycleStage.NotCreated) => new(
                OrderLifecycleStage.Pending,
                transition.OrderId,
                transition.BuyerId,
                transition.OccurredAt
            ),
            (OrderLifecycleStage.Completed, OrderLifecycleStage.Pending) => state with
            {
                Stage = OrderLifecycleStage.Completed,
                TransitionedAt = transition.OccurredAt,
            },
            (OrderLifecycleStage.Cancelled, OrderLifecycleStage.Pending) => state with
            {
                Stage = OrderLifecycleStage.Cancelled,
                TransitionedAt = transition.OccurredAt,
            },
            _ => state,
        };
    }
}

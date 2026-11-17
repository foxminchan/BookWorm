using System.ComponentModel;
using BookWorm.Finance.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

public enum OrderLifecycleAction : byte
{
    [Description("Mark Basket Deleted")]
    MarkBasketDeleted = 0,

    [Description("Fail Basket Deletion")]
    FailBasketDeletion = 1,

    [Description("Complete Order")]
    Complete = 2,

    [Description("Cancel Order")]
    Cancel = 3,

    [Description("Record Timeout Retry")]
    RecordTimeoutRetry = 4,

    [Description("Cancel Order Due to Max Retries")]
    CancelDueToMaxRetries = 5,

    [Description("Fail Order")]
    Fail = 6,
}

public sealed record OrderLifecycleState(
    OrderStatus Status,
    bool BasketDeleted,
    int TimeoutRetryCount,
    int MaxAttempts
);

public sealed record OrderLifecycleCommand(OrderLifecycleAction Action);

public sealed record OrderTransition(
    OrderStatus TargetStatus,
    bool BasketDeleted,
    int TimeoutRetryCount
);

public static class OrderLifecycleDecider
{
    public static readonly Decider<
        OrderLifecycleState,
        OrderTransition
    >.CommandDecider<OrderLifecycleCommand> Instance = new Decider<
        OrderLifecycleState,
        OrderTransition
    >(Evolve, () => new(OrderStatus.Placed, false, 0, 1)).For<OrderLifecycleCommand>(Decide);

    public static bool IsTerminal(OrderStatus status)
    {
        return status
            is OrderStatus.Completed
                or OrderStatus.Cancelled
                or OrderStatus.BasketDeletionFailed
                or OrderStatus.Failed;
    }

    private static OrderTransition[] Decide(
        OrderLifecycleCommand command,
        OrderLifecycleState state
    )
    {
        if (IsTerminal(state.Status))
        {
            throw new FinanceDomainException(
                $"Cannot apply '{command.Action}' because order is already in terminal status '{state.Status}'."
            );
        }

        return command.Action switch
        {
            OrderLifecycleAction.MarkBasketDeleted when state.BasketDeleted =>
                throw new FinanceDomainException(
                    "Cannot mark basket deleted because basket has already been deleted."
                ),
            OrderLifecycleAction.MarkBasketDeleted =>
            [
                new(OrderStatus.Placed, true, state.TimeoutRetryCount),
            ],
            OrderLifecycleAction.FailBasketDeletion =>
            [
                new(OrderStatus.BasketDeletionFailed, state.BasketDeleted, state.TimeoutRetryCount),
            ],
            OrderLifecycleAction.Complete =>
            [
                new(OrderStatus.Completed, state.BasketDeleted, state.TimeoutRetryCount),
            ],
            OrderLifecycleAction.Cancel =>
            [
                new(OrderStatus.Cancelled, state.BasketDeleted, state.TimeoutRetryCount),
            ],
            OrderLifecycleAction.RecordTimeoutRetry =>
            [
                new(OrderStatus.Placed, state.BasketDeleted, state.TimeoutRetryCount + 1),
            ],
            OrderLifecycleAction.CancelDueToMaxRetries
                when state.TimeoutRetryCount < state.MaxAttempts =>
                throw new FinanceDomainException(
                    $"Cannot cancel order due to max retries before reaching {state.MaxAttempts} attempts."
                ),
            OrderLifecycleAction.CancelDueToMaxRetries =>
            [
                new(OrderStatus.Cancelled, state.BasketDeleted, state.TimeoutRetryCount),
            ],
            OrderLifecycleAction.Fail =>
            [
                new(OrderStatus.Failed, state.BasketDeleted, state.TimeoutRetryCount),
            ],
            _ => throw new FinanceDomainException(
                $"Invalid action '{command.Action}' for status '{state.Status}'."
            ),
        };
    }

    private static OrderLifecycleState Evolve(OrderLifecycleState state, OrderTransition transition)
    {
        return state with
        {
            Status = transition.TargetStatus,
            BasketDeleted = transition.BasketDeleted,
            TimeoutRetryCount = transition.TimeoutRetryCount,
        };
    }
}

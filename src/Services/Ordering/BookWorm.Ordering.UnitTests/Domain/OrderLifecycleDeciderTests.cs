using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Exceptions;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderLifecycleDeciderTests
{
    [Test]
    public void GivenPlacedOrder_WhenCompleting_ThenShouldEmitCompletedTransitionAndUpdateState()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var placedAt = DateTimeOffset.UtcNow;
        var completedAt = placedAt.AddMinutes(5);

        var initial = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Pending, placedAt, orderId, buyerId),
        ]);

        var (nextState, transitions) = OrderLifecycleDecider.Instance.Handle(
            new OrderLifecycleCommand(OrderLifecycleAction.Complete, completedAt),
            initial
        );

        transitions.ShouldBe([
            new OrderTransition(OrderLifecycleStage.Completed, completedAt, orderId),
        ]);
        nextState.ShouldBe(
            new OrderLifecycleState(OrderLifecycleStage.Completed, orderId, buyerId, completedAt)
        );
    }

    [Test]
    public void GivenCompletedOrder_WhenCompletingAgain_ThenShouldBeIdempotent()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var now = DateTimeOffset.UtcNow;

        var state = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Pending, now, orderId, buyerId),
            new OrderTransition(OrderLifecycleStage.Completed, now.AddMinutes(1), orderId),
        ]);

        var (nextState, transitions) = OrderLifecycleDecider.Instance.Handle(
            new OrderLifecycleCommand(OrderLifecycleAction.Complete, now.AddMinutes(2)),
            state
        );

        transitions.ShouldBeEmpty();
        nextState.ShouldBe(state);
    }

    [Test]
    public void GivenCancelledOrder_WhenCompleting_ThenShouldThrowOrderingDomainException()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var now = DateTimeOffset.UtcNow;

        var state = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Pending, now, orderId, buyerId),
            new OrderTransition(OrderLifecycleStage.Cancelled, now.AddMinutes(1), orderId),
        ]);

        Should.Throw<OrderingDomainException>(() =>
            OrderLifecycleDecider.Instance.Handle(
                new OrderLifecycleCommand(OrderLifecycleAction.Complete, now.AddMinutes(2)),
                state
            )
        );
    }

    [Test]
    public void GivenNotCreatedOrder_WhenPlacingWithValidIds_ThenShouldEmitPendingTransition()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var placedAt = DateTimeOffset.UtcNow;

        var (nextState, transitions) = OrderLifecycleDecider.Instance.Handle(
            new OrderLifecycleCommand(OrderLifecycleAction.Place, placedAt, orderId, buyerId),
            new OrderLifecycleState(OrderLifecycleStage.NotCreated)
        );

        transitions.ShouldBe([
            new OrderTransition(OrderLifecycleStage.Pending, placedAt, orderId, buyerId),
        ]);
        nextState.ShouldBe(
            new OrderLifecycleState(OrderLifecycleStage.Pending, orderId, buyerId, placedAt)
        );
    }

    [Test]
    public void GivenNotCreatedOrder_WhenPlacingWithoutOrderIdOrBuyerId_ThenShouldThrowOrderingDomainException()
    {
        var now = DateTimeOffset.UtcNow;
        var orderId = OrderId.From(Guid.CreateVersion7());

        Should.Throw<OrderingDomainException>(() =>
            OrderLifecycleDecider.Instance.Handle(
                new OrderLifecycleCommand(OrderLifecycleAction.Place, now),
                new OrderLifecycleState(OrderLifecycleStage.NotCreated)
            )
        );

        Should.Throw<OrderingDomainException>(() =>
            OrderLifecycleDecider.Instance.Handle(
                new OrderLifecycleCommand(OrderLifecycleAction.Place, now, orderId),
                new OrderLifecycleState(OrderLifecycleStage.NotCreated)
            )
        );
    }

    [Test]
    public void GivenPlacedOrder_WhenCancelling_ThenShouldEmitCancelledTransitionAndBeIdempotent()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var placedAt = DateTimeOffset.UtcNow;
        var cancelledAt = placedAt.AddMinutes(5);

        var initial = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Pending, placedAt, orderId, buyerId),
        ]);

        var (cancelledState, transitions) = OrderLifecycleDecider.Instance.Handle(
            new OrderLifecycleCommand(OrderLifecycleAction.Cancel, cancelledAt),
            initial
        );

        transitions.ShouldBe([
            new OrderTransition(OrderLifecycleStage.Cancelled, cancelledAt, orderId),
        ]);
        cancelledState.ShouldBe(
            new OrderLifecycleState(OrderLifecycleStage.Cancelled, orderId, buyerId, cancelledAt)
        );

        var (idempotentState, emptyTransitions) = OrderLifecycleDecider.Instance.Handle(
            new OrderLifecycleCommand(OrderLifecycleAction.Cancel, cancelledAt.AddMinutes(1)),
            cancelledState
        );

        emptyTransitions.ShouldBeEmpty();
        idempotentState.ShouldBe(cancelledState);
    }

    [Test]
    public void GivenCompletedOrder_WhenCancelling_ThenShouldThrowOrderingDomainException()
    {
        var orderId = OrderId.From(Guid.CreateVersion7());
        var buyerId = BuyerId.From(Guid.CreateVersion7());
        var now = DateTimeOffset.UtcNow;

        var state = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Pending, now, orderId, buyerId),
            new OrderTransition(OrderLifecycleStage.Completed, now.AddMinutes(1), orderId),
        ]);

        Should.Throw<OrderingDomainException>(() =>
            OrderLifecycleDecider.Instance.Handle(
                new OrderLifecycleCommand(OrderLifecycleAction.Cancel, now.AddMinutes(2)),
                state
            )
        );
    }

    [Test]
    public void GivenInvalidActionForStage_WhenHandling_ThenShouldThrowOrderingDomainException()
    {
        Should.Throw<OrderingDomainException>(() =>
            OrderLifecycleDecider.Instance.Handle(
                new OrderLifecycleCommand(OrderLifecycleAction.Complete, DateTimeOffset.UtcNow),
                new OrderLifecycleState(OrderLifecycleStage.NotCreated)
            )
        );
    }

    [Test]
    public void GivenInvalidTransition_WhenReconstituting_ThenShouldLeaveStateUnchanged()
    {
        var state = OrderLifecycleDecider.Instance.Reconstitute([
            new OrderTransition(OrderLifecycleStage.Completed, DateTimeOffset.UtcNow),
        ]);

        state.Stage.ShouldBe(OrderLifecycleStage.NotCreated);
    }
}

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
}

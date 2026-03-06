using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.Finance.Saga.Activities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class ActivityLifecycleTests
{
    private static readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

    [Test]
    public void GivenPlaceOrderActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new PlaceOrderActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("place-order"), Times.Once);
    }

    [Test]
    public void GivenCancelOrderActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new CancelOrderActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("cancel-order"), Times.Once);
    }

    [Test]
    public void GivenCompleteOrderActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new CompleteOrderActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("complete-order"), Times.Once);
    }

    [Test]
    public void GivenHandleBasketDeletedActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new HandleBasketDeletedActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("handle-basket-deleted"), Times.Once);
    }

    [Test]
    public void GivenHandleBasketDeleteFailedActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new HandleBasketDeleteFailedActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("handle-basket-delete-failed"), Times.Once);
    }

    [Test]
    public async Task GivenPlaceOrderActivity_WhenFaulted_ThenShouldDelegateToNext()
    {
        var activity = new PlaceOrderActivity(_loggerFactory);
        var context =
            new Mock<
                BehaviorExceptionContext<
                    OrderState,
                    UserCheckedOutIntegrationEvent,
                    InvalidOperationException
                >
            >();
        var next = new Mock<IBehavior<OrderState, UserCheckedOutIntegrationEvent>>();

        await activity.Faulted(context.Object, next.Object);

        next.Verify(n => n.Faulted(context.Object), Times.Once);
    }

    [Test]
    public async Task GivenCancelOrderActivity_WhenFaulted_ThenShouldDelegateToNext()
    {
        var activity = new CancelOrderActivity(_loggerFactory);
        var context =
            new Mock<
                BehaviorExceptionContext<
                    OrderState,
                    OrderStatusChangedToCancelIntegrationEvent,
                    InvalidOperationException
                >
            >();
        var next = new Mock<IBehavior<OrderState, OrderStatusChangedToCancelIntegrationEvent>>();

        await activity.Faulted(context.Object, next.Object);

        next.Verify(n => n.Faulted(context.Object), Times.Once);
    }

    [Test]
    public async Task GivenCompleteOrderActivity_WhenFaulted_ThenShouldDelegateToNext()
    {
        var activity = new CompleteOrderActivity(_loggerFactory);
        var context =
            new Mock<
                BehaviorExceptionContext<
                    OrderState,
                    OrderStatusChangedToCompleteIntegrationEvent,
                    InvalidOperationException
                >
            >();
        var next = new Mock<IBehavior<OrderState, OrderStatusChangedToCompleteIntegrationEvent>>();

        await activity.Faulted(context.Object, next.Object);

        next.Verify(n => n.Faulted(context.Object), Times.Once);
    }

    [Test]
    public async Task GivenHandleBasketDeletedActivity_WhenFaulted_ThenShouldDelegateToNext()
    {
        var activity = new HandleBasketDeletedActivity(_loggerFactory);
        var context =
            new Mock<
                BehaviorExceptionContext<
                    OrderState,
                    BasketDeletedCompleteIntegrationEvent,
                    InvalidOperationException
                >
            >();
        var next = new Mock<IBehavior<OrderState, BasketDeletedCompleteIntegrationEvent>>();

        await activity.Faulted(context.Object, next.Object);

        next.Verify(n => n.Faulted(context.Object), Times.Once);
    }

    [Test]
    public async Task GivenHandleBasketDeleteFailedActivity_WhenFaulted_ThenShouldDelegateToNext()
    {
        var activity = new HandleBasketDeleteFailedActivity(_loggerFactory);
        var context =
            new Mock<
                BehaviorExceptionContext<
                    OrderState,
                    BasketDeletedFailedIntegrationEvent,
                    InvalidOperationException
                >
            >();
        var next = new Mock<IBehavior<OrderState, BasketDeletedFailedIntegrationEvent>>();

        await activity.Faulted(context.Object, next.Object);

        next.Verify(n => n.Faulted(context.Object), Times.Once);
    }
}

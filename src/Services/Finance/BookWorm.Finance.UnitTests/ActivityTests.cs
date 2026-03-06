using BookWorm.Contracts;
using BookWorm.Finance.Saga;
using BookWorm.Finance.Saga.Activities;
using BookWorm.SharedKernel.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class ActivityTests
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
    public async Task GivenPlaceOrderActivity_WhenFaulted_ThenShouldCallNextFaulted()
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
    public void GivenCancelOrderActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new CancelOrderActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("cancel-order"), Times.Once);
    }

    [Test]
    public async Task GivenCancelOrderActivity_WhenFaulted_ThenShouldCallNextFaulted()
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
    public void GivenCompleteOrderActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new CompleteOrderActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("complete-order"), Times.Once);
    }

    [Test]
    public async Task GivenCompleteOrderActivity_WhenFaulted_ThenShouldCallNextFaulted()
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
    public void GivenHandleBasketDeletedActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new HandleBasketDeletedActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("handle-basket-deleted"), Times.Once);
    }

    [Test]
    public async Task GivenHandleBasketDeletedActivity_WhenFaulted_ThenShouldCallNextFaulted()
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
    public void GivenHandleBasketDeleteFailedActivity_WhenProbing_ThenShouldCreateScope()
    {
        var activity = new HandleBasketDeleteFailedActivity(_loggerFactory);
        var probeContext = new Mock<ProbeContext>();

        activity.Probe(probeContext.Object);

        probeContext.Verify(c => c.CreateScope("handle-basket-delete-failed"), Times.Once);
    }

    [Test]
    public async Task GivenHandleBasketDeleteFailedActivity_WhenFaulted_ThenShouldCallNextFaulted()
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

    [Test]
    public async Task GivenUserCheckedOutEvent_WhenExecutingPlaceOrderActivity_ThenShouldMapStateAndSetOrderPlacedDate()
    {
        // Arrange
        var activity = new PlaceOrderActivity(_loggerFactory);
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var message = new UserCheckedOutIntegrationEvent(
            orderId,
            basketId,
            "John Doe",
            "john@example.com",
            99.99m
        );

        var saga = new OrderState();
        var context = CreateBehaviorContext(saga, message);
        var next = Mock.Of<IBehavior<OrderState, UserCheckedOutIntegrationEvent>>();

        // Act
        var before = DateTimeHelper.UtcNow();
        await activity.Execute(context, next);
        var after = DateTimeHelper.UtcNow();

        // Assert
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("John Doe");
        saga.Email.ShouldBe("john@example.com");
        saga.TotalMoney.ShouldBe(99.99m);
        saga.OrderPlacedDate.ShouldNotBeNull();
        saga.OrderPlacedDate.Value.ShouldBeInRange(before, after);

        Mock.Get(next).Verify(n => n.Execute(context), Times.Once);
    }

    [Test]
    public async Task GivenOrderCancelledEvent_WhenExecutingCancelOrderActivity_ThenShouldMapState()
    {
        // Arrange
        var activity = new CancelOrderActivity(_loggerFactory);
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var message = new OrderStatusChangedToCancelIntegrationEvent(
            orderId,
            basketId,
            "Jane",
            "jane@example.com",
            50.00m
        );

        var saga = new OrderState();
        var context = CreateBehaviorContext(saga, message);
        var next = Mock.Of<IBehavior<OrderState, OrderStatusChangedToCancelIntegrationEvent>>();

        // Act
        await activity.Execute(context, next);

        // Assert
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("Jane");
        saga.Email.ShouldBe("jane@example.com");
        saga.TotalMoney.ShouldBe(50.00m);

        Mock.Get(next).Verify(n => n.Execute(context), Times.Once);
    }

    [Test]
    public async Task GivenOrderCompletedEvent_WhenExecutingCompleteOrderActivity_ThenShouldMapState()
    {
        // Arrange
        var activity = new CompleteOrderActivity(_loggerFactory);
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var message = new OrderStatusChangedToCompleteIntegrationEvent(
            orderId,
            basketId,
            "Bob",
            "bob@example.com",
            200.00m
        );

        var saga = new OrderState();
        var context = CreateBehaviorContext(saga, message);
        var next = Mock.Of<IBehavior<OrderState, OrderStatusChangedToCompleteIntegrationEvent>>();

        // Act
        await activity.Execute(context, next);

        // Assert
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.FullName.ShouldBe("Bob");
        saga.Email.ShouldBe("bob@example.com");
        saga.TotalMoney.ShouldBe(200.00m);

        Mock.Get(next).Verify(n => n.Execute(context), Times.Once);
    }

    [Test]
    public async Task GivenBasketDeletedEvent_WhenExecutingHandleBasketDeletedActivity_ThenShouldMapState()
    {
        // Arrange
        var activity = new HandleBasketDeletedActivity(_loggerFactory);
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var message = new BasketDeletedCompleteIntegrationEvent(orderId, basketId, 75.25m);

        var saga = new OrderState();
        var context = CreateBehaviorContext(saga, message);
        var next = Mock.Of<IBehavior<OrderState, BasketDeletedCompleteIntegrationEvent>>();

        // Act
        await activity.Execute(context, next);

        // Assert
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.TotalMoney.ShouldBe(75.25m);

        Mock.Get(next).Verify(n => n.Execute(context), Times.Once);
    }

    [Test]
    public async Task GivenBasketDeleteFailedEvent_WhenExecutingHandleBasketDeleteFailedActivity_ThenShouldMapState()
    {
        // Arrange
        var activity = new HandleBasketDeleteFailedActivity(_loggerFactory);
        var orderId = Guid.CreateVersion7();
        var basketId = Guid.CreateVersion7();
        var message = new BasketDeletedFailedIntegrationEvent(
            orderId,
            basketId,
            "user@example.com",
            100.00m
        );

        var saga = new OrderState();
        var context = CreateBehaviorContext(saga, message);
        var next = Mock.Of<IBehavior<OrderState, BasketDeletedFailedIntegrationEvent>>();

        // Act
        await activity.Execute(context, next);

        // Assert
        saga.OrderId.ShouldBe(orderId);
        saga.BasketId.ShouldBe(basketId);
        saga.Email.ShouldBe("user@example.com");
        saga.TotalMoney.ShouldBe(100.00m);

        Mock.Get(next).Verify(n => n.Execute(context), Times.Once);
    }

    private static BehaviorContext<OrderState, TMessage> CreateBehaviorContext<TMessage>(
        OrderState saga,
        TMessage message
    )
        where TMessage : class
    {
        var context = new Mock<BehaviorContext<OrderState, TMessage>>();
        context.Setup(c => c.Saga).Returns(saga);
        context.Setup(c => c.Message).Returns(message);
        return context.Object;
    }
}

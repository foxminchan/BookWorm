using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Infrastructure.EventStore.Projections;
using BookWorm.Ordering.UnitTests.Fakers;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderSummaryTests
{
    [Test]
    public void GivenOrderPlacedEvent_WhenCreatingOrderSummary_ThenStatusShouldBeNew()
    {
        // Arrange
        var order = new OrderFaker().Generate()[0];
        var orderPlacedEvent = new OrderPlacedEvent(order);

        // Act
        var result = OrderSummary.Create(orderPlacedEvent);

        // Assert
        result.Id.ShouldBe((Guid)order.Id);
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenApplyingToOrderSummary_ThenStatusShouldBeCancelled()
    {
        // Arrange
        var order = new OrderFaker().Generate()[0];
        var summary = OrderSummary.Create(new(order));
        var cancelledEvent = new OrderCancelledEvent(order);

        // Act
        var result = summary.Apply(cancelledEvent);

        // Assert
        result.Id.ShouldBe((Guid)order.Id);
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenApplyingToOrderSummary_ThenStatusShouldBeCompleted()
    {
        // Arrange
        var order = new OrderFaker().Generate()[0];
        var summary = OrderSummary.Create(new(order));
        var completedEvent = new OrderCompletedEvent(order);

        // Act
        var result = summary.Apply(completedEvent);

        // Assert
        result.Id.ShouldBe((Guid)order.Id);
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenEventStream_WhenEvolvingOrderSummary_ThenShouldTransitionThroughStates()
    {
        // Arrange
        var order = new OrderFaker().Generate()[0];
        DomainEvent[] events = [new OrderPlacedEvent(order), new OrderCompletedEvent(order)];

        // Act
        var result = events.Aggregate<DomainEvent, OrderSummary?>(null, OrderSummary.Transition);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe((Guid)order.Id);
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenParameters_WhenCreatingOrderSummary_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        const Status status = Status.New;
        const decimal totalPrice = 150.75m;

        // Act
        var orderSummary = new OrderSummary(orderId, status, totalPrice);

        // Assert
        orderSummary.Id.ShouldBe(orderId);
        orderSummary.Status.ShouldBe(status);
        orderSummary.TotalPrice.ShouldBe(totalPrice);
    }
}

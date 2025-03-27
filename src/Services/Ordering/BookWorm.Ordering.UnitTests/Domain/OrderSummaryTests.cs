using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderSummaryTests
{
    [Test]
    public void GivenOrderSummary_WhenApplyingDeleteBasketCompleteCommand_ThenStatusShouldBeNew()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var orderSummary = new OrderSummary(orderId, Status.New, 100.0m);
        var command = new DeleteBasketCompleteCommand(orderId, 100.0m);

        // Act
        var result = orderSummary.Apply(command);

        // Assert
        result.Id.ShouldBe(orderId);
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(100.0m);
    }

    [Test]
    public void GivenOrderSummary_WhenApplyingOrderCancelledEvent_ThenStatusShouldBeCancelled()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var orderSummary = new OrderSummary(orderId, Status.Cancelled, 100.0m);
        var order = new Order(); // This would need to be properly initialized based on your Order class
        var cancelledEvent = new OrderCancelledEvent(order);

        // Act
        var result = orderSummary.Apply(cancelledEvent);

        // Assert
        result.Id.ShouldBe(orderId);
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(100.0m);
    }

    [Test]
    public void GivenOrderSummary_WhenApplyingOrderCompletedEvent_ThenStatusShouldBeCompleted()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var orderSummary = new OrderSummary(orderId, Status.Completed, 100.0m);
        var order = new Order(); // This would need to be properly initialized based on your Order class
        var completedEvent = new OrderCompletedEvent(order);

        // Act
        var result = orderSummary.Apply(completedEvent);

        // Assert
        result.Id.ShouldBe(orderId);
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(100.0m);
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

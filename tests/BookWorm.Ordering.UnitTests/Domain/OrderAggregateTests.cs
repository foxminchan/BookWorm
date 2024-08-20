using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Events;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderAggregateTests
{
    [Fact]
    public void GivenValidConstructorArguments_ShouldInitializePropertiesCorrectly_WhenCreatingOrder()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        const string note = "Please handle with care.";

        // Act
        var order = new Order(buyerId, note);

        // Assert
        order.BuyerId.Should().Be(buyerId);
        order.Note.Should().Be(note);
        order.Status.Should().Be(Status.Pending);
        order.OrderItems.Should().BeEmpty();
        order.TotalPrice.Should().Be(0m);
    }

    [Fact]
    public void GivenDefaultGuid_ShouldThrowArgumentException_WhenCreatingOrder()
    {
        // Arrange
        var buyerId = Guid.Empty;
        const string note = "Please handle with care.";

        // Act
        Func<Order> act = () => new(buyerId, note);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenOrderItem_ShouldIncreaseOrderItemsCollectionAndCalculateTotalPrice_WhenAddingOrderItem()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), null);
        var bookId = Guid.NewGuid();
        const decimal price = 100m;
        const int quantity = 2;

        // Act
        order.AddOrderItem(bookId, price, quantity);

        // Assert
        order.OrderItems.Should().ContainSingle();
        var orderItem = order.OrderItems.First();
        orderItem.BookId.Should().Be(bookId);
        orderItem.Price.Should().Be(price);
        orderItem.Quantity.Should().Be(quantity);
        order.TotalPrice.Should().Be(price * quantity);
    }

    [Fact]
    public void GivenOrder_StatusShouldBeCompletedAndDomainEventRegistered_WhenMarkingAsCompleted()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), null);

        // Act
        order.MarkAsCompleted();

        // Assert
        order.Status.Should().Be(Status.Completed);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCompletedEvent);
    }

    [Fact]
    public void GivenOrder_StatusShouldBeCanceledAndDomainEventRegistered_WhenMarkingAsCanceled()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), null);

        // Act
        order.MarkAsCanceled();

        // Assert
        order.Status.Should().Be(Status.Canceled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
    }
}

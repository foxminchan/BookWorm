using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Domain.Exceptions;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderDomainEventTests
{
    [Test]
    public void GivenNewOrder_WhenCompleted_ThenShouldRegisterOrderCompletedEvent()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var order = new Order(buyerId, "Test order", [new(Guid.CreateVersion7(), 1, 10.00m)]);
        order.ClearDomainEvents();

        // Act
        order.MarkAsCompleted();

        // Assert
        order.DomainEvents.Count.ShouldBe(1);
        var completedEvent = order.DomainEvents.First() as OrderCompletedEvent;
        completedEvent.ShouldNotBeNull();
        completedEvent.Order.ShouldBe(order);
    }

    [Test]
    public void GivenNewOrder_WhenCancelled_ThenShouldRegisterOrderCancelledEvent()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        var order = new Order(buyerId, "Test order", [new(Guid.CreateVersion7(), 1, 10.00m)]);
        order.ClearDomainEvents();

        // Act
        order.MarkAsCanceled();

        // Assert
        order.DomainEvents.Count.ShouldBe(1);
        var cancelledEvent = order.DomainEvents.First() as OrderCancelledEvent;
        cancelledEvent.ShouldNotBeNull();
        cancelledEvent.Order.ShouldBe(order);
    }

    [Test]
    public void GivenOrder_WhenDeleted_ThenIsDeletedShouldBeTrue()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        order.Delete();

        // Assert
        order.IsDeleted.ShouldBeTrue();
    }

    [Test]
    public void GivenNewOrder_WhenCreated_ThenIsDeletedShouldBeFalse()
    {
        // Arrange & Act
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Assert
        order.IsDeleted.ShouldBeFalse();
    }

    [Test]
    public void GivenOrderWithNote_WhenCreated_ThenNoteShouldBeSet()
    {
        // Arrange
        const string note = "Please deliver before 5 PM";

        // Act
        var order = new Order(Guid.CreateVersion7(), note, []);

        // Assert
        order.Note.ShouldBe(note);
    }

    [Test]
    public void GivenOrderWithNullNote_WhenCreated_ThenNoteShouldBeNull()
    {
        // Arrange & Act
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Assert
        order.Note.ShouldBeNull();
    }

    [Test]
    public void GivenOrderWithNoItems_WhenCalculatingTotalPrice_ThenShouldBeZero()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        var totalPrice = order.TotalPrice;

        // Assert
        totalPrice.ShouldBe(0m);
    }

    [Test]
    public void GivenOrderWithSingleItem_WhenCalculatingTotalPrice_ThenShouldReturnItemTotal()
    {
        // Arrange
        var order = new Order(
            Guid.CreateVersion7(),
            null,
            [new(Guid.CreateVersion7(), 2, 25.00m)]
        );

        // Act
        var totalPrice = order.TotalPrice;

        // Assert
        totalPrice.ShouldBe(50.00m);
    }

    [Test]
    public void GivenOrderItems_WhenAccessing_ThenShouldReturnReadOnlyCollection()
    {
        // Arrange
        List<OrderItem> items = [new(Guid.CreateVersion7(), 1, 10.00m)];
        var order = new Order(Guid.CreateVersion7(), null, items);

        // Act
        var orderItems = order.OrderItems;

        // Assert
        orderItems.ShouldNotBeEmpty();
        orderItems.Count.ShouldBe(1);
        orderItems.ShouldBeAssignableTo<IReadOnlyCollection<OrderItem>>();
    }

    [Test]
    public void GivenCancelledOrder_WhenCancelling_ThenShouldThrowDomainException()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCanceled();

        // Act & Assert
        Should.Throw<OrderingDomainException>(() => order.MarkAsCanceled());
    }
}

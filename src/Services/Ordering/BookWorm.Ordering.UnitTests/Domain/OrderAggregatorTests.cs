﻿using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Domain.Exceptions;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderAggregatorTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingOrderItem_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 2;
        const decimal price = 29.99m;

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);

        // Assert
        orderItem.BookId.ShouldBe(bookId);
        orderItem.Quantity.ShouldBe(quantity);
        orderItem.Price.ShouldBe(price);
    }

    [Test]
    public void GivenZeroQuantity_WhenCreatingOrderItem_ThenShouldThrowException()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 0;
        const decimal price = 29.99m;

        // Act & Assert
        Should
            .Throw<OrderingDomainException>(() => new OrderItem(bookId, quantity, price))
            .Message.ShouldBe("Quantity must be greater than zero");
    }

    [Test]
    public void GivenNegativeQuantity_WhenCreatingOrderItem_ThenShouldThrowException()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = -1;
        const decimal price = 29.99m;

        // Act & Assert
        Should
            .Throw<OrderingDomainException>(() => new OrderItem(bookId, quantity, price))
            .Message.ShouldBe("Quantity must be greater than zero");
    }

    [Test]
    public void GivenNegativePrice_WhenCreatingOrderItem_ThenShouldThrowException()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 1;
        const decimal price = -0.01m;

        // Act & Assert
        Should
            .Throw<OrderingDomainException>(() => new OrderItem(bookId, quantity, price))
            .Message.ShouldBe("Price must be greater than or equal to zero");
    }

    [Test]
    public void GivenZeroPrice_WhenCreatingOrderItem_ThenShouldCreateSuccessfully()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 1;
        const decimal price = 0m;

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);

        // Assert
        orderItem.BookId.ShouldBe(bookId);
        orderItem.Quantity.ShouldBe(quantity);
        orderItem.Price.ShouldBe(price);
    }

    [Test]
    public void GivenNewlyCreatedOrder_WhenCheckingStatus_ThenShouldBeNew()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        var status = order.Status;

        // Assert
        status.ShouldBe(Status.New);
    }

    [Test]
    public void GivenCompletedOrder_WhenCancelling_ThenShouldRemainCompleted()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCompleted();

        // Act
        order.MarkAsCanceled();

        // Assert
        order.Status.ShouldBe(Status.Completed);
    }

    [Test]
    public void GivenNewOrder_WhenCompleting_ThenStatusShouldBeCompleted()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        order.MarkAsCompleted();

        // Assert
        order.Status.ShouldBe(Status.Completed);
    }

    [Test]
    public void GivenNewOrder_WhenCreating_ThenShouldRegisterOrderPlacedEvent()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        const string note = "Test order";
        List<OrderItem> orderItems = [new(Guid.CreateVersion7(), 1, 29.99m)];

        // Act
        var order = new Order(buyerId, note, orderItems);

        // Assert
        order.DomainEvents.ShouldNotBeEmpty();
        order.DomainEvents.First().ShouldBeOfType<OrderPlacedEvent>();
        var orderPlacedEvent = (OrderPlacedEvent)order.DomainEvents.First();
        orderPlacedEvent.Order.ShouldBe(order);
    }

    [Test]
    public void GivenOrderWithItems_WhenCreating_ThenShouldSetOrderItemNavigationProperties()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 1;
        const decimal price = 29.99m;

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);

        // Assert
        orderItem.Order.ShouldBeNull();
        orderItem.OrderId.ShouldBe(Guid.Empty);
    }

    [Test]
    public void GivenOrder_WhenCreating_ThenShouldInitializeBuyerProperty()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();

        // Act
        var order = new Order(buyerId, null, []);

        // Assert
        order.Buyer.ShouldBeNull();
        order.BuyerId.ShouldBe(buyerId);
    }

    [Test]
    public void GivenCancelledOrder_WhenCompleting_ThenShouldRemainCancelled()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCanceled();

        // Act
        order.MarkAsCompleted();

        // Assert
        order.Status.ShouldBe(Status.Cancelled);
    }

    [Test]
    public void GivenOrderItem_WhenCalculatingTotalPrice_ThenShouldReturnPriceTimesQuantity()
    {
        // Arrange
        var bookId = Guid.CreateVersion7();
        const int quantity = 3;
        const decimal price = 15.50m;
        const decimal expectedTotal = 46.50m;

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);
        var totalPrice = orderItem.GetTotalPrice();

        // Assert
        totalPrice.ShouldBe(expectedTotal);
    }

    [Test]
    public void GivenOrderWithItems_WhenCalculatingTotalPrice_ThenShouldReturnSumOfAllItems()
    {
        // Arrange
        var buyerId = Guid.CreateVersion7();
        List<OrderItem> orderItems =
        [
            new(Guid.CreateVersion7(), 2, 10.00m), // 20.00
            new(Guid.CreateVersion7(), 1, 15.50m), // 15.50
            new(Guid.CreateVersion7(), 3, 8.75m), // 26.25
        ];
        const decimal expectedTotal = 61.75m;

        // Act
        var order = new Order(buyerId, null, orderItems);

        // Assert
        order.TotalPrice.ShouldBe(expectedTotal);
    }

    [Test]
    public void GivenNewOrder_WhenMarkingAsCompleted_ThenShouldReturnSameInstance()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        var result = order.MarkAsCompleted();

        // Assert
        result.ShouldBeSameAs(order);
    }

    [Test]
    public void GivenCompletedOrder_WhenMarkingAsCompleted_ThenShouldReturnSameInstance()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCompleted(); // Make it completed first

        // Act
        var result = order.MarkAsCompleted(); // Try to complete again

        // Assert
        result.ShouldBeSameAs(order);
    }

    [Test]
    public void GivenNewOrder_WhenMarkingAsCanceled_ThenShouldReturnSameInstance()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);

        // Act
        var result = order.MarkAsCanceled();

        // Assert
        result.ShouldBeSameAs(order);
    }

    [Test]
    public void GivenCompletedOrder_WhenMarkingAsCanceled_ThenShouldReturnSameInstance()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCompleted(); // Make it completed first

        // Act
        var result = order.MarkAsCanceled(); // Try to cancel

        // Assert
        result.ShouldBeSameAs(order);
    }
}

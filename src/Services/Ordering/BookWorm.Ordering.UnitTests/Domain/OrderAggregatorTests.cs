﻿using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
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
    public void GivenCompletedOrder_WhenCancelling_ThenShouldBeCanceled()
    {
        // Arrange
        var order = new Order(Guid.CreateVersion7(), null, []);
        order.MarkAsCompleted();

        // Act
        order.MarkAsCanceled();

        // Assert
        order.Status.ShouldBe(Status.Cancelled);
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
}

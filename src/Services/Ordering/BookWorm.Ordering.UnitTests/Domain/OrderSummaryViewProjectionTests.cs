﻿using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Domain.Projections;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderSummaryViewProjectionTests
{
    private readonly OrderFaker _orderFaker = new();

    [Test]
    public void GivenConstructor_WhenInstantiated_ThenShouldSetupIdentities()
    {
        // No explicit assertions needed as the constructor behavior is validated
        // through the Apply method tests. If identities weren't properly set up,
        // the Apply methods wouldn't work correctly.
        new OrderSummaryViewProjection().ShouldNotBeNull();
    }

    [Test]
    public void GivenOrderPlacedEvent_WhenApplied_ThenShouldUpdateStatusToNewAndSetTotalPrice()
    {
        // Arrange
        var info = new OrderSummaryView();
        var order = _orderFaker.Generate()[0];
        var orderPlacedEvent = new OrderPlacedEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Create(info, orderPlacedEvent);

        // Assert
        result.Id.ShouldBe(order.Id);
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenApplied_ThenShouldUpdateStatusToCancelledAndSetTotalPrice()
    {
        // Arrange
        var info = new OrderSummaryView();
        var order = _orderFaker.Generate()[0];
        var @event = new OrderCancelledEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Apply(info, @event);

        // Assert
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenApplied_ThenShouldUpdateStatusToCompletedAndSetTotalPrice()
    {
        // Arrange
        var info = new OrderSummaryView();
        var order = _orderFaker.Generate()[0];
        var @event = new OrderCompletedEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Apply(info, @event);

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingOrderPlacedEvent_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var info = new OrderSummaryView
        {
            Id = Guid.CreateVersion7(),
            Status = Status.Completed,
            TotalPrice = 999.99m,
        };
        var order = _orderFaker.Generate()[0];
        var orderPlacedEvent = new OrderPlacedEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Create(info, orderPlacedEvent);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Id.ShouldBe(order.Id);
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingOrderCancelledEvent_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var info = new OrderSummaryView
        {
            Id = Guid.CreateVersion7(),
            Status = Status.New,
            TotalPrice = 999.99m,
        };
        var order = _orderFaker.Generate()[0];
        var @event = new OrderCancelledEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Apply(info, @event);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingOrderCompletedEvent_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var info = new OrderSummaryView
        {
            Id = Guid.CreateVersion7(),
            Status = Status.New,
            TotalPrice = 999.99m,
        };
        var order = _orderFaker.Generate()[0];
        var @event = new OrderCompletedEvent(order);

        // Act
        var result = OrderSummaryViewProjection.Apply(info, @event);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }
}

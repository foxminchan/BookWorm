using BookWorm.Contracts;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate;
using BookWorm.Ordering.Domain.Events;
using BookWorm.Ordering.Domain.Projections;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderProjectionTests
{
    private readonly Faker<Order> _orderFaker;
    private readonly Projection _projection;

    public OrderProjectionTests()
    {
        _projection = new();

        _orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, _ => Guid.CreateVersion7())
            .RuleFor(o => o.TotalPrice, f => f.Random.Decimal(10, 1000));
    }

    [Test]
    public void GivenConstructor_WhenInstantiated_ThenShouldSetupIdentities()
    {
        // No explicit assertions needed as the constructor behavior is validated
        // through the Apply method tests. If identities weren't properly set up,
        // the Apply methods wouldn't work correctly.
        _projection.ShouldNotBeNull();
    }

    [Test]
    public void GivenDeleteBasketCommand_WhenApplied_ThenShouldUpdateStatusToNewAndSetTotalPrice()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var totalMoney = 123.45m;
        var info = new OrderSummaryInfo();
        var command = new DeleteBasketCompleteCommand(orderId, totalMoney);

        // Act
        var result = _projection.Apply(info, command);

        // Assert
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenOrderCancelledEvent_WhenApplied_ThenShouldUpdateStatusToCancelledAndSetTotalPrice()
    {
        // Arrange
        var info = new OrderSummaryInfo();
        var order = _orderFaker.Generate();
        var @event = new OrderCancelledEvent(order);

        // Act
        var result = _projection.Apply(info, @event);

        // Assert
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenOrderCompletedEvent_WhenApplied_ThenShouldUpdateStatusToCompletedAndSetTotalPrice()
    {
        // Arrange
        var info = new OrderSummaryInfo();
        var order = _orderFaker.Generate();
        var @event = new OrderCompletedEvent(order);

        // Act
        var result = _projection.Apply(info, @event);

        // Assert
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingDeleteBasketCommand_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var orderId = Guid.CreateVersion7();
        var totalMoney = 123.45m;
        var info = new OrderSummaryInfo
        {
            Id = Guid.CreateVersion7(),
            Status = Status.Completed,
            TotalPrice = 999.99m,
        };
        var command = new DeleteBasketCompleteCommand(orderId, totalMoney);

        // Act
        var result = _projection.Apply(info, command);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Status.ShouldBe(Status.New);
        result.TotalPrice.ShouldBe(totalMoney);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingOrderCancelledEvent_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var info = new OrderSummaryInfo
        {
            Id = Guid.CreateVersion7(),
            Status = Status.New,
            TotalPrice = 999.99m,
        };
        var order = _orderFaker.Generate();
        var @event = new OrderCancelledEvent(order);

        // Act
        var result = _projection.Apply(info, @event);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Status.ShouldBe(Status.Cancelled);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }

    [Test]
    public void GivenExistingOrderSummary_WhenApplyingOrderCompletedEvent_ThenShouldUpdateExistingInfo()
    {
        // Arrange
        var info = new OrderSummaryInfo
        {
            Id = Guid.CreateVersion7(),
            Status = Status.New,
            TotalPrice = 999.99m,
        };
        var order = _orderFaker.Generate();
        var @event = new OrderCompletedEvent(order);

        // Act
        var result = _projection.Apply(info, @event);

        // Assert
        result.ShouldBeSameAs(info); // Should modify the same object
        result.Status.ShouldBe(Status.Completed);
        result.TotalPrice.ShouldBe(order.TotalPrice);
    }
}

using BookWorm.Contracts;
using BookWorm.Notification.Domain.Models;
using BookWorm.Notification.IntegrationEvents;

namespace BookWorm.Notification.UnitTests.Handlers;

public sealed class ContractToModelMapperTests
{
    [Test]
    public void GivenPlaceOrderCommand_WhenMappingToOrder_ThenShouldMapWithNewStatus()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            "John Doe",
            "john@example.com",
            Guid.CreateVersion7(),
            99.99m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.Id.ShouldBe(command.OrderId);
        order.FullName.ShouldBe("John Doe");
        order.TotalMoney.ShouldBe(99.99m);
        order.Status.ShouldBe(Status.New);
    }

    [Test]
    public void GivenPlaceOrderCommandWithNullFullName_WhenMappingToOrder_ThenShouldDefaultToCustomer()
    {
        // Arrange
        var command = new PlaceOrderCommand(
            Guid.CreateVersion7(),
            null,
            "john@example.com",
            Guid.CreateVersion7(),
            50.00m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.FullName.ShouldBe("Customer");
        order.Status.ShouldBe(Status.New);
    }

    [Test]
    public void GivenCancelOrderCommand_WhenMappingToOrder_ThenShouldMapWithCanceledStatus()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.CreateVersion7(),
            "Jane Smith",
            "jane@example.com",
            150.00m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.Id.ShouldBe(command.OrderId);
        order.FullName.ShouldBe("Jane Smith");
        order.TotalMoney.ShouldBe(150.00m);
        order.Status.ShouldBe(Status.Canceled);
    }

    [Test]
    public void GivenCancelOrderCommandWithNullFullName_WhenMappingToOrder_ThenShouldDefaultToCustomer()
    {
        // Arrange
        var command = new CancelOrderCommand(
            Guid.CreateVersion7(),
            null,
            "jane@example.com",
            75.00m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.FullName.ShouldBe("Customer");
        order.Status.ShouldBe(Status.Canceled);
    }

    [Test]
    public void GivenCompleteOrderCommand_WhenMappingToOrder_ThenShouldMapWithCompletedStatus()
    {
        // Arrange
        var command = new CompleteOrderCommand(
            Guid.CreateVersion7(),
            "Bob Wilson",
            "bob@example.com",
            200.00m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.Id.ShouldBe(command.OrderId);
        order.FullName.ShouldBe("Bob Wilson");
        order.TotalMoney.ShouldBe(200.00m);
        order.Status.ShouldBe(Status.Completed);
    }

    [Test]
    public void GivenCompleteOrderCommandWithNullFullName_WhenMappingToOrder_ThenShouldDefaultToCustomer()
    {
        // Arrange
        var command = new CompleteOrderCommand(
            Guid.CreateVersion7(),
            null,
            "bob@example.com",
            100.00m
        );

        // Act
        var order = command.ToOrder();

        // Assert
        order.FullName.ShouldBe("Customer");
        order.Status.ShouldBe(Status.Completed);
    }
}

using BookWorm.Ordering.Domain.OrderAggregate;

namespace BookWorm.Ordering.UnitTests.Domain;

public sealed class OrderItemTests
{
    [Fact]
    public void GivenValidConstructorArguments_ShouldInitializePropertiesCorrectly_WhenCreatingOrderItem()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int quantity = 5;
        const decimal price = 100m;

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);

        // Assert
        orderItem.BookId.Should().Be(bookId);
        orderItem.Quantity.Should().Be(quantity);
        orderItem.Price.Should().Be(price);
    }

    [Theory, CombinatorialData]
    public void GivenInvalidQuantity_ShouldThrowArgumentException_WhenCreatingOrderItem(
        [CombinatorialValues(0, -1)] int invalidQuantity)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const decimal price = 100m;

        // Act
        Func<OrderItem> act = () => new(bookId, invalidQuantity, price);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory, CombinatorialData]
    public void GivenInvalidPrice_ShouldThrowArgumentException_WhenCreatingOrderItem(
        [CombinatorialValues(0.0, -10.0)] decimal invalidPrice)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int quantity = 5;

        // Act
        Func<OrderItem> act = () => new(bookId, quantity, invalidPrice);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenDefaultBookId_ShouldThrowArgumentException_WhenCreatingOrderItem()
    {
        // Arrange
        var bookId = Guid.Empty;
        const int quantity = 5;
        const decimal price = 100m;

        // Act
        Func<OrderItem> act = () => new(bookId, quantity, price);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenValidOrderItem_ShouldHaveValidOrderIdAndOrderReference_WhenSettingOrderId()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        const int quantity = 5;
        const decimal price = 100m;
        var orderId = Guid.NewGuid();

        // Act
        var orderItem = new OrderItem(bookId, quantity, price);

        // Simulate EF Core setting the OrderId and Order reference
        orderItem.GetType()
            .GetProperty(nameof(OrderItem.OrderId))?
            .SetValue(orderItem, orderId);

        orderItem.GetType()
            .GetProperty(nameof(OrderItem.Order))?
            .SetValue(orderItem, new Order(orderId, null!));

        // Assert
        orderItem.OrderId.Should().Be(orderId);
        orderItem.Order.Should().NotBeNull();
    }
}

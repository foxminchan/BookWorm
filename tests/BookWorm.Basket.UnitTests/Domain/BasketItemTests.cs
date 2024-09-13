namespace BookWorm.Basket.UnitTests.Domain;

public class BasketItemTests
{
    [Fact]
    public void GivenValidIdAndQuantity_ShouldHaveCorrectProperties_WhenCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        const int quantity = 5;

        // Act
        var item = new BasketItem(id, quantity);

        // Assert
        item.Id.Should().Be(id);
        item.Quantity.Should().Be(quantity);
    }

    [Theory]
    [CombinatorialData]
    public void GivenValidQuantity_ShouldSetQuantity_WhenCreated(
        [CombinatorialValues(1, 10, int.MaxValue)]
        int quantity)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var item = new BasketItem(id, quantity);

        // Assert
        item.Quantity.Should().Be(quantity);
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidQuantity_ShouldThrowException_WhenCreated(
        [CombinatorialValues(0, -1, int.MinValue)]
        int quantity)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Func<BasketItem> act = () => new(id, quantity);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GivenItem_ShouldIncreaseCorrectly_WhenIncreaseQuantity()
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), 5);
        const int increaseBy = 3;

        // Act
        item.IncreaseQuantity(increaseBy);

        // Assert
        item.Quantity.Should().Be(8);
    }

    [Theory]
    [CombinatorialData]
    public void GivenInvalidQuantity_ShouldThrowException_WhenIncreaseQuantity(
        [CombinatorialValues(0, -1, int.MinValue)]
        int increaseBy)
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), 5);

        // Act
        var act = () => item.IncreaseQuantity(increaseBy);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(5, 1, 6)]
    public void GivenItem_ShouldHaveExpectedQuantity_WhenIncreaseQuantity(int initialQuantity, int increaseBy,
        int expectedQuantity)
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), initialQuantity);

        // Act
        item.IncreaseQuantity(increaseBy);

        // Assert
        item.Quantity.Should().Be(expectedQuantity);
    }

    [Fact]
    public void GivenItem_ShouldReduceCorrectly_WhenReduceQuantity()
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), 5);
        const int reduceBy = 3;

        // Act
        item.ReduceQuantity(reduceBy);

        // Assert
        item.Quantity.Should().Be(2);
    }

    [Fact]
    public void GivenItem_ShouldSetQuantityToZero_WhenReduceQuantityToNegative()
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), 5);
        const int reduceBy = 10;

        // Act
        item.ReduceQuantity(reduceBy);

        // Assert
        item.Quantity.Should().Be(0);
    }

    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(5, 5, 0)]
    [InlineData(5, 6, 0)]
    public void GivenItem_ShouldHaveExpectedQuantity_WhenReduceQuantity(int initialQuantity, int reduceBy,
        int expectedQuantity)
    {
        // Arrange
        var item = new BasketItem(Guid.NewGuid(), initialQuantity);

        // Act
        item.ReduceQuantity(reduceBy);

        // Assert
        item.Quantity.Should().Be(expectedQuantity);
    }
}

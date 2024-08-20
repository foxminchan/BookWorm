using BookWorm.Basket.Domain;
using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.UnitTests.Domain;

public sealed class BasketTests
{
    [Fact]
    public void GivenNewBasket_ShouldHaveCorrectAccountIdAndItems_WhenInitialized()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var basketItems = new List<BasketItem>();

        // Act
        var basket = new BasketModel(accountId, basketItems);

        // Assert
        basket.AccountId.Should().Be(accountId);
        basket.BasketItems.Should().BeEmpty();
    }

    [Fact]
    public void GivenItem_ShouldContainItem_WhenAddedToBasket()
    {
        // Arrange
        var basket = new BasketModel(Guid.NewGuid(), []);
        var item = new BasketItem(Guid.NewGuid(), 1);

        // Act
        basket.AddItem(item);

        // Assert
        basket.BasketItems.Should().ContainSingle(x => x.Id == item.Id && x.Quantity == item.Quantity);
    }

    [Fact]
    public void GivenExistingItem_ShouldIncreaseQuantity_WhenAddedToBasket()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var basket = new BasketModel(Guid.NewGuid(), [new(itemId, 1)]);
        var item = new BasketItem(itemId, 2);

        // Act
        basket.AddItem(item);

        // Assert
        basket.BasketItems.Should().ContainSingle(x => x.Id == itemId && x.Quantity == 3);
    }

    [Fact]
    public void GivenExistingItem_ShouldNotContainItem_WhenRemovedFromBasket()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var basket = new BasketModel(Guid.NewGuid(), [new(itemId, 1)]);

        // Act
        basket.RemoveItem(itemId);

        // Assert
        basket.BasketItems.Should().NotContain(x => x.Id == itemId);
    }

    [Fact]
    public void GivenNonExistingItem_ShouldDoNothing_WhenRemovedFromBasket()
    {
        // Arrange
        var basket = new BasketModel(Guid.NewGuid(), [new(Guid.NewGuid(), 1)]);
        var nonExistingItemId = Guid.NewGuid();

        // Act
        basket.RemoveItem(nonExistingItemId);

        // Assert
        basket.BasketItems.Should().HaveCount(1);
    }

    [Fact]
    public void GivenItem_ShouldDecreaseQuantity_WhenReducedQuantity()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var basket = new BasketModel(Guid.NewGuid(), [new(itemId, 3)]);

        // Act
        basket.ReduceItemQuantity(itemId, 2);

        // Assert
        basket.BasketItems.Should().ContainSingle(x => x.Id == itemId && x.Quantity == 1);
    }

    [Fact]
    public void GivenItem_ShouldRemoveItem_WhenReducedQuantityToZero()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var basket = new BasketModel(Guid.NewGuid(), [new(itemId, 3)]);

        // Act
        basket.ReduceItemQuantity(itemId, 3);

        // Assert
        basket.BasketItems.Should().NotContain(x => x.Id == itemId);
    }

    [Fact]
    public void GivenNonExistingItem_ShouldDoNothing_WhenReducedQuantity()
    {
        // Arrange
        var basket = new BasketModel(Guid.NewGuid(), [new(Guid.NewGuid(), 3)]);
        var nonExistingItemId = Guid.NewGuid();

        // Act
        basket.ReduceItemQuantity(nonExistingItemId, 1);

        // Assert
        basket.BasketItems.Should().HaveCount(1);
    }
}

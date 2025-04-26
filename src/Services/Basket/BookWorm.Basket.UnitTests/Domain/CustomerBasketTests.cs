using BookWorm.Basket.Domain;
using BookWorm.Basket.Infrastructure.Exceptions;

namespace BookWorm.Basket.UnitTests.Domain;

public sealed class CustomerBasketTests
{
    [Test]
    public void GivenIdAndItems_WhenCreatingCustomerBasket_ThenPropertiesShouldBeSet()
    {
        // Arrange
        const string id = "customer-123";
        var items = new List<BasketItem> { new("book-1", 2), new("book-2", 1) };

        // Act
        var basket = new CustomerBasket(id, items);

        // Assert
        basket.Id.ShouldBe(id);
        basket.Items.Count.ShouldBe(2);
        basket.Items.ShouldContain(i => i.Id == "book-1" && i.Quantity == 2);
        basket.Items.ShouldContain(i => i.Id == "book-2" && i.Quantity == 1);
    }

    [Test]
    public void GivenDefaultConstructor_WhenCreatingCustomerBasket_ThenShouldHaveEmptyItems()
    {
        // Act
        var basket = new CustomerBasket();

        // Assert
        basket.Id.ShouldBeNull();
        basket.Items.ShouldBeEmpty();
    }

    [Test]
    public void GivenNewItems_WhenUpdatingBasket_ThenItemsShouldBeReplaced()
    {
        // Arrange
        var basket = new CustomerBasket("customer-123", [new BasketItem("book-1", 2)]);

        var newItems = new List<BasketItem> { new("book-3", 1), new("book-4", 3) };

        // Act
        basket.Update(newItems);

        // Assert
        basket.Items.Count.ShouldBe(2);
        basket.Items.ShouldContain(i => i.Id == "book-3" && i.Quantity == 1);
        basket.Items.ShouldContain(i => i.Id == "book-4" && i.Quantity == 3);
        basket.Items.ShouldNotContain(i => i.Id == "book-1");
    }

    [Test]
    public void GivenEmptyList_WhenUpdatingBasket_ThenItemsShouldBeEmpty()
    {
        // Arrange
        var basket = new CustomerBasket(
            "customer-123",
            [new BasketItem("book-1", 2), new BasketItem("book-2", 1)]
        );

        // Act
        basket.Update([]);

        // Assert
        basket.Items.ShouldBeEmpty();
    }

    [Test]
    public void GivenNullId_WhenCreatingCustomerBasket_ThenShouldThrowException()
    {
        // Arrange
        const string? id = default;
        var items = new List<BasketItem> { new("book-1", 2), new("book-2", 1) };

        // Act & Assert
        Should
            .Throw<BasketDomainException>(() => new CustomerBasket(id!, items))
            .Message.ShouldBe("Customer ID cannot be null.");
    }

    [Test]
    public void GivenEmptyItems_WhenCreatingCustomerBasket_ThenShouldThrowException()
    {
        // Arrange
        const string id = "customer-123";
        var items = new List<BasketItem>();

        // Act & Assert
        Should
            .Throw<BasketDomainException>(() => new CustomerBasket(id, items))
            .Message.ShouldBe("Basket must contain at least one item.");
    }
}

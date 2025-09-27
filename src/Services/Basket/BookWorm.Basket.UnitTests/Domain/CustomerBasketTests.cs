﻿using BookWorm.Basket.Domain;
using BookWorm.Basket.Infrastructure.Exceptions;

namespace BookWorm.Basket.UnitTests.Domain;

public sealed class CustomerBasketTests
{
    [Test]
    public void GivenIdAndItems_WhenCreatingCustomerBasket_ThenPropertiesShouldBeSet()
    {
        // Arrange
        const string id = "B";
        List<BasketItem> items = [new("book-1", 2), new("book-2", 1)];

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
        var basket = new CustomerBasket(Guid.CreateVersion7().ToString(), [new("book-1", 2)]);

        List<BasketItem> newItems = [new("book-3", 1), new("book-4", 3)];

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
            Guid.CreateVersion7().ToString(),
            [new("book-1", 2), new("book-2", 1)]
        );

        // Act
        basket.Update([]);

        // Assert
        basket.Items.ShouldBeEmpty();
    }

    [Test]
    public void GivenItems_WhenUpdatingBasket_ThenShouldReturnSameInstance()
    {
        // Arrange
        var basket = new CustomerBasket(Guid.CreateVersion7().ToString(), [new("book-1", 2)]);

        List<BasketItem> newItems = [new("book-3", 1)];

        // Act
        var result = basket.Update(newItems);

        // Assert
        result.ShouldBeSameAs(basket); // Verifies it's the same object instance
    }

    [Test]
    public void GivenNullId_WhenCreatingCustomerBasket_ThenShouldThrowException()
    {
        // Arrange
        const string? id = null;
        List<BasketItem> items = [new("book-1", 2), new("book-2", 1)];

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
        List<BasketItem> items = [];

        // Act & Assert
        Should
            .Throw<BasketDomainException>(() => new CustomerBasket(id, items))
            .Message.ShouldBe("Basket must contain at least one item.");
    }
}

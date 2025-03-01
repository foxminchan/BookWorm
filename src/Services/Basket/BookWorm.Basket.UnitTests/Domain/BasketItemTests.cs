using System.ComponentModel.DataAnnotations;
using BookWorm.Basket.Domain;

namespace BookWorm.Basket.UnitTests.Domain;

public sealed class BasketItemTests
{
    [Test]
    public void GivenIdAndQuantity_WhenCreatingBasketItem_ThenPropertiesShouldBeSet()
    {
        // Arrange
        const string id = "book-123";
        const int quantity = 5;

        // Act
        var basketItem = new BasketItem(id, quantity);

        // Assert
        basketItem.Id.ShouldBe(id);
        basketItem.Quantity.ShouldBe(quantity);
    }

    [Test]
    public void GivenDefaultConstructor_WhenCreatingBasketItem_ThenPropertiesShouldBeDefault()
    {
        // Act
        var basketItem = new BasketItem();

        // Assert
        basketItem.Id.ShouldBeNull();
        basketItem.Quantity.ShouldBe(0);
    }

    [Test]
    public void GivenValidQuantity_WhenValidatingBasketItem_ThenNoValidationErrorsShouldBeReturned()
    {
        // Arrange
        var basketItem = new BasketItem("book-1", 3);
        var validationContext = new ValidationContext(basketItem);

        // Act
        var validationResults = basketItem.Validate(validationContext).ToList();

        // Assert
        validationResults.ShouldBeEmpty();
    }

    [Test]
    public void GivenZeroQuantity_WhenValidatingBasketItem_ThenValidationErrorShouldBeReturned()
    {
        // Arrange
        var basketItem = new BasketItem("book-1", 0);
        var validationContext = new ValidationContext(basketItem);

        // Act
        var validationResults = basketItem.Validate(validationContext).ToList();

        // Assert
        validationResults.Count.ShouldBe(1);
        validationResults[0].ErrorMessage.ShouldBe("Quantity must be greater than zero.");
        validationResults[0].MemberNames.ShouldContain("Quantity");
    }

    [Test]
    public void GivenNegativeQuantity_WhenValidatingBasketItem_ThenValidationErrorShouldBeReturned()
    {
        // Arrange
        var basketItem = new BasketItem("book-1", -5);
        var validationContext = new ValidationContext(basketItem);

        // Act
        var validationResults = basketItem.Validate(validationContext).ToList();

        // Assert
        validationResults.Count.ShouldBe(1);
        validationResults[0].ErrorMessage.ShouldBe("Quantity must be greater than zero.");
        validationResults[0].MemberNames.ShouldContain("Quantity");
    }
}

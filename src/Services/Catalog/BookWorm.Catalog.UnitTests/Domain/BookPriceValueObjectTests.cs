using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.Exceptions;

namespace BookWorm.Catalog.UnitTests.Domain;

public sealed class BookPriceValueObjectTests
{
    [Test]
    public void GivenValidPrices_WhenCreatingPrice_ThenShouldCreatePriceSuccessfully()
    {
        // Act
        var price = new Price(100.0m, 80.0m);

        // Assert
        price.OriginalPrice.ShouldBe(100.0m);
        price.DiscountPrice.ShouldBe(80.0m);
    }

    [Test]
    public void GivenValidPriceWithoutDiscount_WhenCreatingPrice_ThenShouldCreatePriceWithNullDiscount()
    {
        // Act
        var price = new Price(100.0m, null);

        // Assert
        price.OriginalPrice.ShouldBe(100.0m);
        price.DiscountPrice.ShouldBeNull();
    }

    [Test]
    public void GivenNegativeOriginalPrice_WhenCreatingPrice_ThenShouldThrowDomainException()
    {
        // Act
        Func<Price> act = () => new(-10.0m, null);

        // Assert
        act.ShouldThrow<CatalogDomainException>()
            .Message.ShouldContain("Original price must be greater than or equal to 0");
    }

    [Test]
    public void GivenNegativeDiscountPrice_WhenCreatingPrice_ThenShouldThrowDomainException()
    {
        // Act
        Func<Price> act = () => new(100.0m, -10.0m);

        // Assert
        act.ShouldThrow<CatalogDomainException>()
            .Message.ShouldContain("Discount price must be greater than or equal to 0");
    }

    [Test]
    public void GivenDiscountPriceGreaterThanOriginalPrice_WhenCreatingPrice_ThenShouldThrowDomainException()
    {
        // Act
        Func<Price> act = () => new(100.0m, 150.0m);

        // Assert
        act.ShouldThrow<CatalogDomainException>()
            .Message.ShouldContain(
                "Discount price must be greater than or equal to 0 and less than or equal to original price"
            );
    }

    [Test]
    public void GivenTwoEqualPrices_WhenComparingEquality_ThenShouldBeEqual()
    {
        // Arrange
        var price1 = new Price(100.0m, 80.0m);
        var price2 = new Price(100.0m, 80.0m);

        // Act & Assert
        price1.ShouldBe(price2);
    }

    [Test]
    public void GivenTwoDifferentPrices_WhenComparingEquality_ThenShouldNotBeEqual()
    {
        // Arrange
        var price1 = new Price(100.0m, 80.0m);
        var price2 = new Price(100.0m, 70.0m);

        // Act & Assert
        price1.ShouldNotBe(price2);
    }

    [Test]
    public void GivenTwoPricesWithSameOriginalButDifferentDiscount_WhenComparingEquality_ThenShouldNotBeEqual()
    {
        // Arrange
        var price1 = new Price(100.0m, 80.0m);
        var price2 = new Price(100.0m, null);

        // Act & Assert
        price1.ShouldNotBe(price2);
        price1.Equals(price2).ShouldBeFalse();
    }
}

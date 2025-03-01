using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Create;
using FluentValidation.TestHelper;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketValidatorTests
{
    private readonly CreateBasketValidator _validator = new();

    [Test]
    public void GivenValidItems_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateBasketCommand(
            [new BasketItemRequest("item1", 1), new BasketItemRequest("item2", 2)]
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyItemsList_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBasketCommand([]);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Test]
    public void GivenItemWithEmptyId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBasketCommand([new BasketItemRequest("", 1)]);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Id");
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public void GivenItemWithInvalidQuantity_WhenValidating_ThenShouldHaveValidationError(
        int invalidQuantity
    )
    {
        // Arrange
        var command = new CreateBasketCommand([new BasketItemRequest("item1", invalidQuantity)]);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Test]
    public void GivenMultipleValidationErrors_WhenValidating_ThenShouldHaveAllValidationErrors()
    {
        // Arrange
        var command = new CreateBasketCommand(
            [new BasketItemRequest("", 0), new BasketItemRequest("item2", -1)]
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Id");
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
        result.ShouldHaveValidationErrorFor("Items[1].Quantity");
    }
}

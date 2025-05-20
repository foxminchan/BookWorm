using BookWorm.Constants.Core;
using BookWorm.Ordering.Features.Buyers.Create;
using FluentValidation.TestHelper;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Create;

public sealed class CreateBuyerValidatorTests
{
    private readonly CreateBuyerValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyStreet_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand("", "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Test]
    public void GivenNullStreet_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand(null!, "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Test]
    public void GivenTooLongStreet_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand(
            new('A', DataSchemaLength.Medium + 1),
            "New York",
            "NY"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Test]
    public void GivenEmptyCity_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void GivenNullCity_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", null!, "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void GivenTooLongCity_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand(
            "123 Main St",
            new('A', DataSchemaLength.Medium + 1),
            "NY"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void GivenEmptyProvince_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "New York", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province);
    }

    [Test]
    public void GivenNullProvince_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "New York", null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province);
    }

    [Test]
    public void GivenTooLongProvince_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateBuyerCommand(
            "123 Main St",
            "New York",
            new('A', DataSchemaLength.Medium + 1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province);
    }
}

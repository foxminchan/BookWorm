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
    [Arguments("")]
    [Arguments(null)]
    public void GivenBlankStreet_WhenValidating_ThenShouldHaveValidationError(string? street)
    {
        // Arrange
        var command = new CreateBuyerCommand(street!, "New York", "NY");

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
    [Arguments("")]
    [Arguments(null)]
    public void GivenBlankCity_WhenValidating_ThenShouldHaveValidationError(string? city)
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", city!, "NY");

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
    [Arguments("")]
    [Arguments(null)]
    public void GivenBlankProvince_WhenValidating_ThenShouldHaveValidationError(string? province)
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "New York", province!);

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

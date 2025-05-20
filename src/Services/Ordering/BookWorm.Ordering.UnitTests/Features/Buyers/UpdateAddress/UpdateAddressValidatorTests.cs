using BookWorm.Constants.Core;
using BookWorm.Ordering.Features.Buyers.UpdateAddress;
using FluentValidation.TestHelper;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressValidatorTests
{
    private readonly UpdateAddressValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateAddressCommand("123 Main St", "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyStreet_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateAddressCommand("", "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Test]
    public void GivenEmptyCity_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateAddressCommand("123 Main St", "", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void GivenEmptyProvince_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateAddressCommand("123 Main St", "New York", "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province);
    }

    [Test]
    public void GivenStreetExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longStreet = new string('A', DataSchemaLength.Medium + 1);
        var command = new UpdateAddressCommand(longStreet, "New York", "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Test]
    public void GivenCityExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longCity = new string('A', DataSchemaLength.Medium + 1);
        var command = new UpdateAddressCommand("123 Main St", longCity, "NY");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Test]
    public void GivenProvinceExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longProvince = new string('A', DataSchemaLength.Medium + 1);
        var command = new UpdateAddressCommand("123 Main St", "New York", longProvince);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province);
    }
}

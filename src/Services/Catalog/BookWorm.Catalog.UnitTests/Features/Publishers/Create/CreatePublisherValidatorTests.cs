using BookWorm.Catalog.Features.Publishers.Create;
using BookWorm.Constants;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Create;

public sealed class CreatePublisherValidatorTests
{
    private readonly CreatePublisherValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreatePublisherCommand("O'Reilly");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    public void GivenEmptyOrNullName_WhenValidating_ThenShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new CreatePublisherCommand(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreatePublisherCommand(new('a', DataSchemaLength.Large + 1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

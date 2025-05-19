using BookWorm.Catalog.Features.Authors.Create;
using BookWorm.Constants;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Create;

public sealed class CreateAuthorValidatorTests
{
    private readonly CreateAuthorValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateAuthorCommand("John Doe");

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
        var command = new CreateAuthorCommand(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateAuthorCommand(new('a', DataSchemaLength.Large + 1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

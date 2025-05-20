using BookWorm.Catalog.Features.Authors.Update;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Update;

public sealed class UpdateAuthorValidatorTests
{
    private readonly UpdateAuthorValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateAuthorCommand(Guid.CreateVersion7(), "Valid Author Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateAuthorCommand(Guid.Empty, "Valid Author Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    public void GivenEmptyOrNullName_WhenValidating_ThenShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new UpdateAuthorCommand(Guid.CreateVersion7(), name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('a', DataSchemaLength.Large + 1);
        var command = new UpdateAuthorCommand(Guid.CreateVersion7(), longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

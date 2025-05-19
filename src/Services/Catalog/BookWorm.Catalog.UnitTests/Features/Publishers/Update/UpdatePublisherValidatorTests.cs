using BookWorm.Catalog.Features.Publishers.Update;
using BookWorm.Constants;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Update;

public sealed class UpdatePublisherValidatorTests
{
    private readonly UpdatePublisherValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdatePublisherCommand(Guid.CreateVersion7(), "Valid Publisher Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePublisherCommand(Guid.Empty, "Valid Publisher Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenEmptyOrWhitespaceName_WhenValidating_ThenShouldHaveValidationError(
        string? name
    )
    {
        // Arrange
        var command = new UpdatePublisherCommand(Guid.CreateVersion7(), name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdatePublisherCommand(
            Guid.CreateVersion7(),
            new('A', DataSchemaLength.Medium + 1)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

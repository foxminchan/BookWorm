using BookWorm.Chat.Features.Create;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Chat.UnitTests.Features.Create;

public sealed class CreateChatValidatorTests
{
    private readonly CreateChatValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateChatCommand("Valid Chat Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("   ")]
    public void GivenEmptyOrNullName_WhenValidating_ThenShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new CreateChatCommand(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', DataSchemaLength.Large + 1);
        var command = new CreateChatCommand(longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameAtMaxLength_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var maxLengthName = new string('A', DataSchemaLength.Large);
        var command = new CreateChatCommand(maxLengthName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameJustOverMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var overMaxLengthName = new string('B', DataSchemaLength.Large + 1);
        var command = new CreateChatCommand(overMaxLengthName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(
                $"The length of 'Name' must be {DataSchemaLength.Large} characters or fewer. You entered {DataSchemaLength.Large + 1} characters."
            );
    }

    [Test]
    public void GivenValidChatNameWithSpecialCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateChatCommand("Chat-Name_123!@#");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenValidChatNameWithUnicodeCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateChatCommand("Chat 聊天 💬");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

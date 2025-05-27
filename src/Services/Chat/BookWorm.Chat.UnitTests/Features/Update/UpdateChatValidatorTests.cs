using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Update;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Chat.UnitTests.Features.Update;

public sealed class UpdateChatValidatorTests
{
    private readonly UpdateChatValidator _validator = new();

    [Test]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var prompt = new Prompt("What is the best selling book in BookWorm?");
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenEmptyId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var prompt = new Prompt("Valid prompt text");
        var command = new UpdateChatCommand(Guid.Empty, prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("   ")]
    public void GivenEmptyOrNullPromptText_WhenValidating_ThenShouldHaveValidationError(
        string? promptText
    )
    {
        // Arrange
        var prompt = new Prompt(promptText!);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenPromptTextExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longPromptText = new string('A', DataSchemaLength.Max + 1);
        var prompt = new Prompt(longPromptText);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenPromptTextAtMaxLength_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var maxLengthPromptText = new string('A', DataSchemaLength.Max);
        var prompt = new Prompt(maxLengthPromptText);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenPromptTextJustOverMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var overMaxLengthPromptText = new string('B', DataSchemaLength.Max + 1);
        var prompt = new Prompt(overMaxLengthPromptText);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Prompt.Text)
            .WithErrorMessage(
                $"The length of 'Prompt Text' must be {DataSchemaLength.Max} characters or fewer. You entered {DataSchemaLength.Max + 1} characters."
            );
    }

    [Test]
    public void GivenValidPromptTextWithSpecialCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var prompt = new Prompt("What about émojis 🚀 and spëcial chars!?");
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenValidPromptTextWithUnicodeCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var prompt = new Prompt("Chat 聊天 💬 question about books");
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenBothEmptyIdAndEmptyPromptText_WhenValidating_ThenShouldHaveValidationErrorsForBoth()
    {
        // Arrange
        var prompt = new Prompt("");
        var command = new UpdateChatCommand(Guid.Empty, prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenValidIdButPromptTextExceedingMaxLength_WhenValidating_ThenShouldOnlyHavePromptTextValidationError()
    {
        // Arrange
        var longPromptText = new string('A', DataSchemaLength.Max + 1);
        var prompt = new Prompt(longPromptText);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenEmptyIdButValidPromptText_WhenValidating_ThenShouldOnlyHaveIdValidationError()
    {
        // Arrange
        var prompt = new Prompt("Valid prompt text");
        var command = new UpdateChatCommand(Guid.Empty, prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenMultipleCommandsWithDifferentPromptLengths_WhenValidating_ThenShouldValidateCorrectly()
    {
        // Arrange
        var shortPromptCommand = new UpdateChatCommand(Guid.CreateVersion7(), new("Short"));
        var maxPromptCommand = new UpdateChatCommand(
            Guid.CreateVersion7(),
            new(new('A', DataSchemaLength.Max))
        );
        var longPromptCommand = new UpdateChatCommand(
            Guid.CreateVersion7(),
            new(new('A', DataSchemaLength.Max + 1))
        );

        // Act
        var shortResult = _validator.TestValidate(shortPromptCommand);
        var maxResult = _validator.TestValidate(maxPromptCommand);
        var longResult = _validator.TestValidate(longPromptCommand);

        // Assert
        shortResult.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
        maxResult.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
        longResult.ShouldHaveValidationErrorFor(x => x.Prompt.Text);
    }

    [Test]
    public void GivenCommandWithMinimalValidInput_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var prompt = new Prompt("A");
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenCommandWithLongValidPrompt_WhenValidating_ThenShouldNotHaveValidationErrors()
    {
        // Arrange
        var longValidPrompt = new string('A', 500);
        var prompt = new Prompt(longValidPrompt);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenCommandWithNewLineAndTabCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var promptWithSpecialChars = "What is the\nbest selling\tbook in BookWorm?";
        var prompt = new Prompt(promptWithSpecialChars);
        var command = new UpdateChatCommand(Guid.CreateVersion7(), prompt);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Prompt.Text);
    }
}

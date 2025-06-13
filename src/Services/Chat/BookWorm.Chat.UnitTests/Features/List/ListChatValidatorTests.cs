using BookWorm.Chat.Features.List;
using BookWorm.Constants.Core;
using FluentValidation.TestHelper;

namespace BookWorm.Chat.UnitTests.Features.List;

public sealed class ListChatValidatorTests
{
    private readonly ListChatValidator _validator = new();

    [Test]
    public void GivenValidQuery_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var query = new ListChatQuery("Valid Chat Name", Guid.CreateVersion7(), true);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenDefaultQuery_WhenValidating_ThenShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var query = new ListChatQuery();

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void GivenNullName_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery(null, Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenEmptyName_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("", Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenWhitespaceName_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("   ", Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameAtMaxLength_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var maxLengthName = new string('A', DataSchemaLength.Large);
        var query = new ListChatQuery(maxLengthName, Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameExceedingMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', DataSchemaLength.Large + 1);
        var query = new ListChatQuery(longName, Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNameJustOverMaxLength_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var overMaxLengthName = new string('B', DataSchemaLength.Large + 1);
        var query = new ListChatQuery(overMaxLengthName, Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(
                $"The length of 'Name' must be {DataSchemaLength.Large} characters or fewer. You entered {DataSchemaLength.Large + 1} characters."
            );
    }

    [Test]
    public void GivenValidNameWithSpecialCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("Chat-Name_123!@#", Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenValidNameWithUnicodeCharacters_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("Chat 聊天 💬", Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void GivenNullUserId_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("Chat Name");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Test]
    public void GivenEmptyGuidUserId_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("Chat Name", Guid.Empty);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Test]
    public void GivenValidUserId_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var query = new ListChatQuery("Chat Name", Guid.CreateVersion7());

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void GivenIncludeMessagesFlag_WhenValidating_ThenShouldNotHaveValidationError(
        bool includeMessages
    )
    {
        // Arrange
        var query = new ListChatQuery("Chat Name", Guid.CreateVersion7(), includeMessages);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IncludeMessages);
    }

    [Test]
    public void GivenMultipleQueriesWithDifferentNameLengths_WhenValidating_ThenShouldValidateCorrectly()
    {
        // Arrange
        var shortNameQuery = new ListChatQuery("A", Guid.CreateVersion7());
        var maxNameQuery = new ListChatQuery(
            new('A', DataSchemaLength.Large),
            Guid.CreateVersion7()
        );
        var longNameQuery = new ListChatQuery(
            new('A', DataSchemaLength.Large + 1),
            Guid.CreateVersion7()
        );

        // Act
        var shortResult = _validator.TestValidate(shortNameQuery);
        var maxResult = _validator.TestValidate(maxNameQuery);
        var longResult = _validator.TestValidate(longNameQuery);

        // Assert
        shortResult.ShouldNotHaveValidationErrorFor(x => x.Name);
        maxResult.ShouldNotHaveValidationErrorFor(x => x.Name);
        longResult.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

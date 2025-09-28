using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Domain.Exceptions;

namespace BookWorm.Chat.UnitTests.Domain;

public sealed class ConversationMessageTests
{
    [Test]
    public void GivenValidParameters_WhenCreatingConversationMessage_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Hello, World!";
        const string role = "user";
        var parentMessageId = Guid.CreateVersion7();

        // Act
        var message = new ConversationMessage(id, text, role, parentMessageId);

        // Assert
        message.Id.ShouldBe(id);
        message.Text.ShouldBe(text);
        message.Role.ShouldBe(role);
        message.ParentMessageId.ShouldBe(parentMessageId);
        message.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Test]
    public void GivenNullIdAndValidParameters_WhenCreatingConversationMessage_ThenShouldGenerateNewId()
    {
        // Arrange
        Guid? id = null;
        const string text = "Hello, World!";
        const string role = "user";

        // Act
        var message = new ConversationMessage(id, text, role);

        // Assert
        message.Id.ShouldNotBe(Guid.Empty);
        message.Text.ShouldBe(text);
        message.Role.ShouldBe(role);
        message.ParentMessageId.ShouldBeNull();
    }

    [Test]
    public void GivenValidParametersWithoutParentMessageId_WhenCreatingConversationMessage_ThenShouldSetParentMessageIdToNull()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Hello, World!";
        const string role = "user";

        // Act
        var message = new ConversationMessage(id, text, role);

        // Assert
        message.Id.ShouldBe(id);
        message.Text.ShouldBe(text);
        message.Role.ShouldBe(role);
        message.ParentMessageId.ShouldBeNull();
    }

    [Test]
    public void GivenNullText_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        string? text = null;
        const string role = "user";

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text!, role)
        );
        exception.Message.ShouldBe("Text cannot be null or empty.");
    }

    [Test]
    public void GivenEmptyText_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var text = string.Empty;
        const string role = "user";

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text, role)
        );
        exception.Message.ShouldBe("Text cannot be null or empty.");
    }

    [Test]
    public void GivenWhitespaceOnlyText_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "   ";
        const string role = "user";

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text, role)
        );
        exception.Message.ShouldBe("Text cannot be null or empty.");
    }

    [Test]
    public void GivenNullRole_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Hello, World!";
        string? role = null;

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text, role!)
        );
        exception.Message.ShouldBe("Role cannot be null or empty.");
    }

    [Test]
    public void GivenEmptyRole_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Hello, World!";
        var role = string.Empty;

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text, role)
        );
        exception.Message.ShouldBe("Role cannot be null or empty.");
    }

    [Test]
    public void GivenWhitespaceOnlyRole_WhenCreatingConversationMessage_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Hello, World!";
        const string role = "   ";

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new ConversationMessage(id, text, role)
        );
        exception.Message.ShouldBe("Role cannot be null or empty.");
    }

    [Test]
    public void GivenParameterlessConstructor_WhenCreatingConversationMessage_ThenShouldInitializeCorrectly()
    {
        // Arrange & Act
        var message = new ConversationMessage();

        // Assert
        message.Text.ShouldBeNull();
        message.Role.ShouldBeNull();
        message.ParentMessageId.ShouldBeNull();
        message.Id.ShouldBe(Guid.Empty);
    }

    [Test]
    public void GivenDifferentRoleValues_WhenCreatingConversationMessage_ThenShouldAcceptValidRoles()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string text = "Test message";
        var roles = new[] { "user", "assistant", "system", "function" };

        foreach (var role in roles)
        {
            // Act
            var message = new ConversationMessage(id, text, role);

            // Assert
            message.Role.ShouldBe(role);
        }
    }

    [Test]
    public void GivenLongTextContent_WhenCreatingConversationMessage_ThenShouldAcceptLongText()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var longText = new string('a', 10000); // 10k characters
        const string role = "user";

        // Act
        var message = new ConversationMessage(id, longText, role);

        // Assert
        message.Text.ShouldBe(longText);
        message.Text!.Length.ShouldBe(10000);
    }
}

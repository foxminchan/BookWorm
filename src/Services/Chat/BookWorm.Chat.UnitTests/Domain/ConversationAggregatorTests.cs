using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Domain.Exceptions;

namespace BookWorm.Chat.UnitTests.Domain;

public sealed class ConversationAggregatorTests
{
    [Test]
    public void GivenValidNameAndUserId_WhenCreatingConversation_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string name = "Test Conversation";
        var userId = Guid.CreateVersion7();

        // Act
        var conversation = new Conversation(name, userId);

        // Assert
        conversation.Name.ShouldBe(name);
        conversation.UserId.ShouldBe(userId);
        conversation.Messages.ShouldBeEmpty();
        conversation.Id.ShouldNotBe(Guid.Empty);
        conversation.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        conversation.Version.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public void GivenValidNameAndNullUserId_WhenCreatingConversation_ThenShouldSetPropertiesCorrectly()
    {
        // Arrange
        const string name = "Test Conversation";
        Guid? userId = null;

        // Act
        var conversation = new Conversation(name, userId);

        // Assert
        conversation.Name.ShouldBe(name);
        conversation.UserId.ShouldBeNull();
        conversation.Messages.ShouldBeEmpty();
        conversation.Id.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public void GivenNullName_WhenCreatingConversation_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        string? name = null;
        var userId = Guid.CreateVersion7();

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new Conversation(name!, userId)
        );
        exception.Message.ShouldBe("Name cannot be null or empty.");
    }

    [Test]
    public void GivenEmptyName_WhenCreatingConversation_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        var name = string.Empty;
        var userId = Guid.CreateVersion7();

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new Conversation(name, userId)
        );
        exception.Message.ShouldBe("Name cannot be null or empty.");
    }

    [Test]
    public void GivenWhitespaceOnlyName_WhenCreatingConversation_ThenShouldThrowConversationDomainException()
    {
        // Arrange
        const string name = "   ";
        var userId = Guid.CreateVersion7();

        // Act & Assert
        var exception = Should.Throw<ConversationDomainException>(() =>
            new Conversation(name, userId)
        );
        exception.Message.ShouldBe("Name cannot be null or empty.");
    }

    [Test]
    public void GivenValidMessage_WhenAddingMessage_ThenShouldAddToMessagesCollection()
    {
        // Arrange
        var conversation = new Conversation("Test Conversation", Guid.CreateVersion7());
        var message = new ConversationMessage(Guid.CreateVersion7(), "Hello, World!", "user");

        // Act
        conversation.AddMessage(message);

        // Assert
        conversation.Messages.ShouldHaveSingleItem();
        conversation.Messages.First().ShouldBe(message);
    }

    [Test]
    public void GivenMultipleMessages_WhenAddingMessages_ThenShouldAddAllToMessagesCollection()
    {
        // Arrange
        var conversation = new Conversation("Test Conversation", Guid.CreateVersion7());
        var message1 = new ConversationMessage(Guid.CreateVersion7(), "First message", "user");
        var message2 = new ConversationMessage(
            Guid.CreateVersion7(),
            "Second message",
            "assistant"
        );
        var message3 = new ConversationMessage(Guid.CreateVersion7(), "Third message", "user");

        // Act
        conversation.AddMessage(message1);
        conversation.AddMessage(message2);
        conversation.AddMessage(message3);

        // Assert
        conversation.Messages.Count.ShouldBe(3);
        conversation.Messages.ShouldContain(message1);
        conversation.Messages.ShouldContain(message2);
        conversation.Messages.ShouldContain(message3);
    }

    [Test]
    public void GivenConversation_WhenAccessingMessages_ThenShouldReturnReadOnlyCollection()
    {
        // Arrange
        var conversation = new Conversation("Test Conversation", Guid.CreateVersion7());

        // Act
        var messages = conversation.Messages;

        // Assert
        messages.ShouldBeAssignableTo<IReadOnlyCollection<ConversationMessage>>();
    }

    [Test]
    public void GivenParameterlessConstructor_WhenCreatingConversation_ThenShouldInitializeCorrectly()
    {
        // Arrange & Act
        var conversation = new Conversation();

        // Assert
        conversation.Name.ShouldBeNull();
        conversation.UserId.ShouldBeNull();
        conversation.Messages.ShouldBeEmpty();
        conversation.Id.ShouldNotBe(Guid.Empty);
    }
}

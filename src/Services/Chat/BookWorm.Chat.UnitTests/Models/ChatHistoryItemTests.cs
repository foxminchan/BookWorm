using BookWorm.Chat.Models;

namespace BookWorm.Chat.UnitTests.Models;

public sealed class ChatHistoryItemTests
{
    [Test]
    public void GivenNewInstance_WhenCreated_ThenShouldInitializeWithDefaultValues()
    {
        // Act
        var chatHistoryItem = new ChatHistoryItem();

        // Assert
        chatHistoryItem.Key.ShouldBeNull();
        chatHistoryItem.ThreadId.ShouldBeNull();
        chatHistoryItem.Timestamp.ShouldBeNull();
        chatHistoryItem.SerializedMessage.ShouldBeNull();
        chatHistoryItem.MessageText.ShouldBeNull();
        chatHistoryItem.UserId.ShouldBeNull();
    }

    [Test]
    public void GivenValidKey_WhenSettingKey_ThenShouldStoreKeyCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string key = "test-key-123";

        // Act
        chatHistoryItem.Key = key;

        // Assert
        chatHistoryItem.Key.ShouldBe(key);
    }

    [Test]
    public void GivenValidThreadId_WhenSettingThreadId_ThenShouldStoreThreadIdCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string threadId = "thread-456";

        // Act
        chatHistoryItem.ThreadId = threadId;

        // Assert
        chatHistoryItem.ThreadId.ShouldBe(threadId);
    }

    [Test]
    public void GivenValidTimestamp_WhenSettingTimestamp_ThenShouldStoreTimestampCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        chatHistoryItem.Timestamp = timestamp;

        // Assert
        chatHistoryItem.Timestamp.ShouldBe(timestamp);
    }

    [Test]
    public void GivenValidSerializedMessage_WhenSettingSerializedMessage_ThenShouldStoreSerializedMessageCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string serializedMessage = "{\"role\":\"user\",\"content\":\"Hello\"}";

        // Act
        chatHistoryItem.SerializedMessage = serializedMessage;

        // Assert
        chatHistoryItem.SerializedMessage.ShouldBe(serializedMessage);
    }

    [Test]
    public void GivenValidMessageText_WhenSettingMessageText_ThenShouldStoreMessageTextCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string messageText = "Hello, how can I help you?";

        // Act
        chatHistoryItem.MessageText = messageText;

        // Assert
        chatHistoryItem.MessageText.ShouldBe(messageText);
    }

    [Test]
    public void GivenValidUserId_WhenSettingUserId_ThenShouldStoreUserIdCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string userId = "user-789";

        // Act
        chatHistoryItem.UserId = userId;

        // Assert
        chatHistoryItem.UserId.ShouldBe(userId);
    }

    [Test]
    public void GivenAllProperties_WhenSettingAllProperties_ThenShouldStoreAllPropertiesCorrectly()
    {
        // Arrange
        const string key = "test-key-123";
        const string threadId = "thread-456";
        var timestamp = DateTimeOffset.UtcNow;
        const string serializedMessage = "{\"role\":\"assistant\",\"content\":\"Hi there!\"}";
        const string messageText = "Hi there!";
        const string userId = "user-789";

        // Act
        var chatHistoryItem = new ChatHistoryItem
        {
            Key = key,
            ThreadId = threadId,
            Timestamp = timestamp,
            SerializedMessage = serializedMessage,
            MessageText = messageText,
            UserId = userId,
        };

        // Assert
        chatHistoryItem.Key.ShouldBe(key);
        chatHistoryItem.ThreadId.ShouldBe(threadId);
        chatHistoryItem.Timestamp.ShouldBe(timestamp);
        chatHistoryItem.SerializedMessage.ShouldBe(serializedMessage);
        chatHistoryItem.MessageText.ShouldBe(messageText);
        chatHistoryItem.UserId.ShouldBe(userId);
    }

    [Test]
    public void GivenNullValues_WhenSettingProperties_ThenShouldAllowNullValues()
    {
        // Arrange & Act
        var chatHistoryItem = new ChatHistoryItem
        {
            Key = null,
            ThreadId = null,
            Timestamp = null,
            SerializedMessage = null,
            MessageText = null,
            UserId = null,
        };

        // Assert
        chatHistoryItem.Key.ShouldBeNull();
        chatHistoryItem.ThreadId.ShouldBeNull();
        chatHistoryItem.Timestamp.ShouldBeNull();
        chatHistoryItem.SerializedMessage.ShouldBeNull();
        chatHistoryItem.MessageText.ShouldBeNull();
        chatHistoryItem.UserId.ShouldBeNull();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    [Arguments("valid-key-with-dashes")]
    [Arguments("KEY123")]
    public void GivenVariousKeyFormats_WhenSettingKey_ThenShouldAcceptAllFormats(string key)
    {
        // Arrange & Act
        var chatHistoryItem = new ChatHistoryItem { Key = key };

        // Assert
        chatHistoryItem.Key.ShouldBe(key);
    }

    [Test]
    public void GivenDifferentTimeZones_WhenSettingTimestamp_ThenShouldHandleTimestampCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        var utcTimestamp = new DateTimeOffset(2025, 10, 4, 12, 0, 0, TimeSpan.Zero);
        var localTimestamp = new DateTimeOffset(2025, 10, 4, 12, 0, 0, TimeSpan.FromHours(5));

        // Act
        chatHistoryItem.Timestamp = utcTimestamp;
        var firstTimestamp = chatHistoryItem.Timestamp;

        chatHistoryItem.Timestamp = localTimestamp;
        var secondTimestamp = chatHistoryItem.Timestamp;

        // Assert
        firstTimestamp.ShouldBe(utcTimestamp);
        secondTimestamp.ShouldBe(localTimestamp);
        firstTimestamp.ShouldNotBe(secondTimestamp);
    }

    [Test]
    [Arguments("simple-thread")]
    [Arguments("thread-with-uuid-550e8400-e29b-41d4-a716-446655440000")]
    [Arguments("THREAD_UPPERCASE")]
    public void GivenVariousThreadIdFormats_WhenSettingThreadId_ThenShouldAcceptAllFormats(
        string threadId
    )
    {
        // Arrange & Act
        var chatHistoryItem = new ChatHistoryItem { ThreadId = threadId };

        // Assert
        chatHistoryItem.ThreadId.ShouldBe(threadId);
    }

    [Test]
    public void GivenComplexSerializedMessage_WhenSettingSerializedMessage_ThenShouldStoreCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        const string complexMessage =
            "{\"role\":\"assistant\",\"content\":\"Multi-line\\n message\",\"metadata\":{\"tokens\":150}}";

        // Act
        chatHistoryItem.SerializedMessage = complexMessage;

        // Assert
        chatHistoryItem.SerializedMessage.ShouldBe(complexMessage);
    }

    [Test]
    public void GivenLongMessageText_WhenSettingMessageText_ThenShouldStoreCorrectly()
    {
        // Arrange
        var chatHistoryItem = new ChatHistoryItem();
        var longMessage = new string('A', 1000);

        // Act
        chatHistoryItem.MessageText = longMessage;

        // Assert
        chatHistoryItem.MessageText.ShouldBe(longMessage);
        chatHistoryItem.MessageText.Length.ShouldBe(1000);
    }

    [Test]
    public void GivenEmptyStrings_WhenSettingProperties_ThenShouldAcceptEmptyStrings()
    {
        // Arrange & Act
        var chatHistoryItem = new ChatHistoryItem
        {
            Key = string.Empty,
            ThreadId = string.Empty,
            SerializedMessage = string.Empty,
            MessageText = string.Empty,
            UserId = string.Empty,
        };

        // Assert
        chatHistoryItem.Key.ShouldBeEmpty();
        chatHistoryItem.ThreadId.ShouldBeEmpty();
        chatHistoryItem.SerializedMessage.ShouldBeEmpty();
        chatHistoryItem.MessageText.ShouldBeEmpty();
        chatHistoryItem.UserId.ShouldBeEmpty();
    }

    [Test]
    public void GivenMultipleUpdates_WhenUpdatingProperties_ThenShouldRetainLatestValue()
    {
        // Arrange & Act
        var chatHistoryItem = new ChatHistoryItem { Key = "first-key" };

        chatHistoryItem.Key = "second-key";
        chatHistoryItem.Key = "final-key";

        // Assert
        chatHistoryItem.Key.ShouldBe("final-key");
    }
}

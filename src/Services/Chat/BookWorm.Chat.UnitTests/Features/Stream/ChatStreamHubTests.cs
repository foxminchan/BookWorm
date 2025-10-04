using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Stream;
using BookWorm.Chat.Infrastructure.ChatStreaming;
using Microsoft.AspNetCore.SignalR;

namespace BookWorm.Chat.UnitTests.Features.Stream;

public sealed class ChatStreamHubTests
{
    private readonly Mock<IChatStreaming> _chatStreamingMock = new();

    private static ChatStreamHub CreateHub()
    {
        return new();
    }

    [Test]
    public async Task GivenValidParameters_WhenStreamingMessages_ThenShouldReturnMessageFragments()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var lastMessageId = Guid.CreateVersion7();
        var lastFragmentId = Guid.CreateVersion7();
        var streamContext = new StreamContext(lastMessageId, lastFragmentId);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>
        {
            new(Guid.CreateVersion7(), "Assistant", "Hello", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", " world", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", "!", Guid.CreateVersion7(), true),
        };

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(
                    conversationId,
                    lastMessageId,
                    lastFragmentId,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(3);
        resultList[0].Text.ShouldBe("Hello");
        resultList[1].Text.ShouldBe(" world");
        resultList[2].Text.ShouldBe("!");
        resultList[2].IsFinal.ShouldBe(true);
    }

    [Test]
    public async Task GivenNullStreamContext_WhenStreamingMessages_ThenShouldPassNullValuesToService()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var streamContext = new StreamContext(null, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>
        {
            new(Guid.CreateVersion7(), "Assistant", "Test", Guid.CreateVersion7(), true),
        };

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>())
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(1);
        _chatStreamingMock.Verify(
            s => s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyStream_WhenStreamingMessages_ThenShouldReturnEmptyResult()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var streamContext = new StreamContext(null, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>();

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>())
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(0);
    }

    [Test]
    public async Task GivenLargeNumberOfFragments_WhenStreamingMessages_ThenShouldStreamAllFragments()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var streamContext = new StreamContext(null, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        const int fragmentCount = 1000;
        var fragments = Enumerable
            .Range(0, fragmentCount)
            .Select(i => new ClientMessageFragment(
                Guid.CreateVersion7(),
                "Assistant",
                $"Fragment {i}",
                Guid.CreateVersion7(),
                i == fragmentCount - 1
            ))
            .ToList();

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>())
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(fragmentCount);
        resultList[^1].IsFinal.ShouldBe(true);
    }

    [Test]
    public async Task GivenPartialStreamContext_WhenStreamingMessages_ThenShouldResumeFromCorrectPosition()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var lastMessageId = Guid.CreateVersion7();
        var streamContext = new StreamContext(lastMessageId, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>
        {
            new(Guid.CreateVersion7(), "Assistant", "Resumed", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", " message", Guid.CreateVersion7(), true),
        };

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(
                    conversationId,
                    lastMessageId,
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(2);
        _chatStreamingMock.Verify(
            s =>
                s.GetMessageStream(
                    conversationId,
                    lastMessageId,
                    null,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public void GivenChatStreamHub_WhenCheckingType_ThenShouldBeCorrectType()
    {
        // Arrange & Act
        var hub = CreateHub();

        // Assert
        hub.ShouldNotBeNull();
        hub.ShouldBeOfType<ChatStreamHub>();
        hub.ShouldBeAssignableTo<Hub>();
    }

    [Test]
    public async Task GivenStreamWithSpecialCharacters_WhenStreamingMessages_ThenShouldHandleCorrectly()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var streamContext = new StreamContext(null, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>
        {
            new(Guid.CreateVersion7(), "Assistant", "你好", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", " 🌍", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", " مرحبا", Guid.CreateVersion7(), true),
        };

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>())
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList.Count.ShouldBe(3);
        resultList[0].Text.ShouldBe("你好");
        resultList[1].Text.ShouldBe(" 🌍");
        resultList[2].Text.ShouldBe(" مرحبا");
    }

    [Test]
    public async Task GivenMultipleSenders_WhenStreamingMessages_ThenShouldPreserveSenderIdentity()
    {
        // Arrange
        var conversationId = Guid.CreateVersion7();
        var streamContext = new StreamContext(null, null);
        using var cancellationTokenSource = new CancellationTokenSource();

        var fragments = new List<ClientMessageFragment>
        {
            new(Guid.CreateVersion7(), "User", "Hello", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "Assistant", "Hi there", Guid.CreateVersion7()),
            new(Guid.CreateVersion7(), "User", "How are you?", Guid.CreateVersion7(), true),
        };

        _chatStreamingMock
            .Setup(s =>
                s.GetMessageStream(conversationId, null, null, It.IsAny<CancellationToken>())
            )
            .Returns(fragments.ToAsyncEnumerable());

        // Act
        var hub = CreateHub();
        var result = hub.Stream(
            conversationId,
            streamContext,
            _chatStreamingMock.Object,
            cancellationTokenSource.Token
        );

        // Assert
        var resultList = await result.ToListAsync(cancellationTokenSource.Token);
        resultList[0].Sender.ShouldBe("User");
        resultList[1].Sender.ShouldBe("Assistant");
        resultList[2].Sender.ShouldBe("User");
    }
}

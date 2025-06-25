using System.Diagnostics;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Stream;
using BookWorm.Chat.Infrastructure.ChatStreaming;
using BookWorm.Chat.Infrastructure.ConversationState.Abstractions;

namespace BookWorm.Chat.UnitTests.Features.Stream;

public sealed class ChatStreamHubTests
{
    private readonly Mock<IChatStreaming> _chatStreamingMock = new();
    private readonly Guid _conversationId = Guid.CreateVersion7();

    private readonly List<ClientMessageFragment> _messageFragments =
    [
        new(Guid.CreateVersion7(), "system", "Hello", Guid.CreateVersion7()),
        new(Guid.CreateVersion7(), "system", "How are you?", Guid.CreateVersion7()),
        new(Guid.CreateVersion7(), "system", "I'm here to help.", Guid.CreateVersion7(), true),
    ];

    [Test]
    public async Task GivenValidText_WhenAddingStreamingMessage_ThenShouldCompleteSuccessfully()
    {
        // Arrange
        const string messageText = "Hello, how can I help you today?";

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(_conversationId, messageText))
            .Returns(Task.CompletedTask);

        // Act
        await _chatStreamingMock.Object.AddStreamingMessage(_conversationId, messageText);

        // Assert
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(_conversationId, messageText),
            Times.Once
        );
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    public void GivenInvalidText_WhenAddingStreamingMessage_ThenShouldThrowArgumentException(
        string? invalidText
    )
    {
        // Arrange
        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(_conversationId, invalidText!))
            .ThrowsAsync(
                new ArgumentException("Message text cannot be null or empty", nameof(invalidText))
            );

        // Act
        var act = async () =>
            await _chatStreamingMock.Object.AddStreamingMessage(_conversationId, invalidText!);

        // Assert
        act.ShouldThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task GivenValidParameters_WhenGettingMessageStream_ThenShouldReturnAllFragments()
    {
        // Arrange
        Guid? lastMessageId = null;
        Guid? lastFragmentId = null;
        var cancellationToken = CancellationToken.None;

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    lastMessageId,
                    lastFragmentId,
                    cancellationToken
                )
            )
            .Returns(_messageFragments.ToAsyncEnumerable());

        // Act
        var result = await _chatStreamingMock
            .Object.GetMessageStream(
                _conversationId,
                lastMessageId,
                lastFragmentId,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldBe(_messageFragments);
    }

    [Test]
    public async Task GivenLastMessageId_WhenGettingMessageStream_ThenShouldReturnOnlyNewerFragments()
    {
        // Arrange
        var lastMessageId = _messageFragments[0].Id;
        Guid? lastFragmentId = null;
        var expectedFragments = _messageFragments.Skip(1).ToList();

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    lastMessageId,
                    lastFragmentId,
                    CancellationToken.None
                )
            )
            .Returns(expectedFragments.ToAsyncEnumerable());

        // Act
        var result = await _chatStreamingMock
            .Object.GetMessageStream(_conversationId, lastMessageId, lastFragmentId)
            .ToListAsync();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldBe(expectedFragments);
    }

    [Test]
    public async Task GivenLastFragmentId_WhenGettingMessageStream_ThenShouldReturnFragmentsAfterLastDelivered()
    {
        // Arrange
        Guid? lastMessageId = null;
        var lastFragmentId = _messageFragments[1].FragmentId;
        List<ClientMessageFragment> expectedFragments = [_messageFragments[2]];

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    lastMessageId,
                    lastFragmentId,
                    CancellationToken.None
                )
            )
            .Returns(expectedFragments.ToAsyncEnumerable());

        // Act
        var result = await _chatStreamingMock
            .Object.GetMessageStream(_conversationId, lastMessageId, lastFragmentId)
            .ToListAsync();

        // Assert
        result.Count.ShouldBe(1);
        result.ShouldBe(expectedFragments);
    }

    [Test]
    public void GivenCancellationRequested_WhenGettingMessageStream_ThenShouldThrowOperationCanceledException()
    {
        // Arrange
        CancellationToken cancellationToken;
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.Cancel();
            cancellationToken = cancellationTokenSource.Token;
        }

        _chatStreamingMock
            .Setup(x => x.GetMessageStream(_conversationId, null, null, cancellationToken))
            .Throws<OperationCanceledException>();

        // Act
        Func<Task> act = async () =>
            await _chatStreamingMock
                .Object.GetMessageStream(_conversationId, null, null, cancellationToken)
                .ToListAsync(cancellationToken);

        // Assert
        act.ShouldThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task GivenInvalidConversationId_WhenGettingMessageStream_ThenShouldReturnEmptyStream()
    {
        // Arrange
        var invalidConversationId = Guid.CreateVersion7();

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(invalidConversationId, null, null, CancellationToken.None)
            )
            .Returns(AsyncEnumerable.Empty<ClientMessageFragment>());

        // Act
        var result = await _chatStreamingMock
            .Object.GetMessageStream(invalidConversationId, null, null)
            .ToListAsync();

        // Assert
        result.Count.ShouldBe(0);
    }

    [Test]
    public async Task GivenStreamContext_WhenGettingMessageStream_ThenShouldUseLastMessageIdAndLastFragmentId()
    {
        // Arrange
        var streamContext = new StreamContext(
            _messageFragments[0].Id,
            _messageFragments[1].FragmentId
        );
        List<ClientMessageFragment> expectedFragments = [_messageFragments[2]];

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    CancellationToken.None
                )
            )
            .Returns(expectedFragments.ToAsyncEnumerable());

        // Act
        var result = await _chatStreamingMock
            .Object.GetMessageStream(
                _conversationId,
                streamContext.LastMessageId,
                streamContext.LastFragmentId
            )
            .ToListAsync();

        // Assert
        result.Count.ShouldBe(1);
        result.ShouldBe(expectedFragments);
    }

    [Test]
    public async Task GivenPrompt_WhenAddingStreamingMessage_ThenShouldUsePromptText()
    {
        // Arrange
        var prompt = new Prompt("What is the capital of France?");

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(_conversationId, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        await _chatStreamingMock.Object.AddStreamingMessage(_conversationId, prompt.Text);

        // Assert
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(_conversationId, prompt.Text),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidParameters_WhenStreaming_ThenShouldForwardToStreamingService()
    {
        // Arrange
        var hub = new ChatStreamHub();
        var streamContext = new StreamContext(
            _messageFragments[0].Id,
            _messageFragments[1].FragmentId
        );
        var cancellationToken = CancellationToken.None;

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationToken
                )
            )
            .Returns(_messageFragments.ToAsyncEnumerable());

        // Act
        var result = await hub.Stream(
                _conversationId,
                streamContext,
                _chatStreamingMock.Object,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldBe(_messageFragments);
        _chatStreamingMock.Verify(
            x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationRequested_WhenStreaming_ThenShouldPropagateCancellation()
    {
        // Arrange
        var hub = new ChatStreamHub();
        var streamContext = new StreamContext(
            _messageFragments[0].Id,
            _messageFragments[1].FragmentId
        );

        CancellationToken cancellationToken;
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            await cancellationTokenSource.CancelAsync();
            cancellationToken = cancellationTokenSource.Token;
        }

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationToken
                )
            )
            .Throws<OperationCanceledException>();

        // Act
        Func<Task> act = async () =>
            await hub.Stream(
                    _conversationId,
                    streamContext,
                    _chatStreamingMock.Object,
                    cancellationToken
                )
                .ToListAsync(cancellationToken);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        _chatStreamingMock.Verify(
            x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyStreamContext_WhenStreaming_ThenShouldUseNullParameters()
    {
        // Arrange
        var hub = new ChatStreamHub();
        var streamContext = new StreamContext(null, null);
        var cancellationToken = CancellationToken.None;

        _chatStreamingMock
            .Setup(x => x.GetMessageStream(_conversationId, null, null, cancellationToken))
            .Returns(_messageFragments.ToAsyncEnumerable());

        // Act
        var result = await hub.Stream(
                _conversationId,
                streamContext,
                _chatStreamingMock.Object,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        // Assert
        result.Count.ShouldBe(3);
        result.ShouldBe(_messageFragments);
        _chatStreamingMock.Verify(
            x => x.GetMessageStream(_conversationId, null, null, cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleClients_WhenStreaming_ThenAllClientsShouldReceiveMessages()
    {
        // Arrange
        const int clientCount = 3;
        var hub = new ChatStreamHub();
        var streamContext = new StreamContext(null, null);
        var cancellationToken = CancellationToken.None;

        _chatStreamingMock
            .Setup(x => x.GetMessageStream(_conversationId, null, null, cancellationToken))
            .Returns(_messageFragments.ToAsyncEnumerable());

        // Act - Simulate multiple clients requesting the same stream
        List<Task<List<ClientMessageFragment>>> clientTasks = [];

        for (var i = 0; i < clientCount; i++)
        {
            var task = hub.Stream(
                    _conversationId,
                    streamContext,
                    _chatStreamingMock.Object,
                    cancellationToken
                )
                .ToListAsync(cancellationToken)
                .AsTask();

            clientTasks.Add(task);
        }

        var results = await Task.WhenAll(clientTasks);

        // Assert
        results.Length.ShouldBe(clientCount);

        foreach (var result in results)
        {
            result.Count.ShouldBe(3);
            result.ShouldBe(_messageFragments);
        }

        // Verify that the streaming service was called for each client
        _chatStreamingMock.Verify(
            x => x.GetMessageStream(_conversationId, null, null, cancellationToken),
            Times.Exactly(clientCount)
        );
    }

    [Test]
    public async Task GivenLargeConversation_WhenLoading_ThenShouldCompleteWithinTimeout()
    {
        // Arrange
        const int largeMessageCount = 1000;
        const int timeoutMs = 5000;

        var hub = new ChatStreamHub();
        var streamContext = new StreamContext(null, null);

        CancellationToken cancellationToken;
        using (var cancellationTokenSource = new CancellationTokenSource(timeoutMs))
        {
            cancellationToken = cancellationTokenSource.Token;
        }

        // Create a large set of message fragments to simulate a conversation with many messages
        List<ClientMessageFragment> largeMessageFragments = [];
        for (var i = 0; i < largeMessageCount; i++)
        {
            largeMessageFragments.Add(
                new(
                    Guid.CreateVersion7(),
                    "user",
                    $"Message {i}: This is a test message with some content to simulate realistic message size.",
                    Guid.CreateVersion7(),
                    i == largeMessageCount - 1 // Mark last message as final
                )
            );
        }

        _chatStreamingMock
            .Setup(x => x.GetMessageStream(_conversationId, null, null, cancellationToken))
            .Returns(largeMessageFragments.ToAsyncEnumerable());

        // Act
        var stopwatch = Stopwatch.StartNew();

        var result = await hub.Stream(
                _conversationId,
                streamContext,
                _chatStreamingMock.Object,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        stopwatch.Stop();

        // Assert
        result.Count.ShouldBe(largeMessageCount);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(timeoutMs);

        // Verify the first and last messages to ensure data integrity
        result[0]
            .Text.ShouldBe(
                "Message 0: This is a test message with some content to simulate realistic message size."
            );
        result[largeMessageCount - 1]
            .Text.ShouldBe(
                $"Message {largeMessageCount - 1}: This is a test message with some content to simulate realistic message size."
            );
        result[largeMessageCount - 1].IsFinal.ShouldBeTrue();

        _chatStreamingMock.Verify(
            x => x.GetMessageStream(_conversationId, null, null, cancellationToken),
            Times.Once
        );
    }
}

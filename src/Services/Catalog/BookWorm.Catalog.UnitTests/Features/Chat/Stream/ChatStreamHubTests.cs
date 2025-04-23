using BookWorm.Catalog.Features.Chat;
using BookWorm.Catalog.Features.Chat.Stream;
using BookWorm.Catalog.Infrastructure.GenAi.ChatStreaming;
using BookWorm.Catalog.Infrastructure.GenAi.ConversationState.Abstractions;

namespace BookWorm.Catalog.UnitTests.Features.Chat.Stream;

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
    public async Task GivenValidText_WhenAddingStreamingMessage_ThenShouldReturnMessageId()
    {
        // Arrange
        const string messageText = "Hello, how can I help you today?";
        var expectedMessageId = Guid.CreateVersion7();

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(messageText))
            .ReturnsAsync(expectedMessageId);

        // Act
        var result = await _chatStreamingMock.Object.AddStreamingMessage(messageText);

        // Assert
        result.ShouldBe(expectedMessageId);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(messageText), Times.Once);
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
            .Setup(x => x.AddStreamingMessage(invalidText!))
            .ThrowsAsync(
                new ArgumentException("Message text cannot be null or empty", nameof(invalidText))
            );

        // Act
        Func<Task> act = async () =>
            await _chatStreamingMock.Object.AddStreamingMessage(invalidText!);

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
        var expectedFragments = new List<ClientMessageFragment> { _messageFragments[2] };

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
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(_conversationId, null, null, cancellationTokenSource.Token)
            )
            .Throws<OperationCanceledException>();

        // Act
        Func<Task> act = async () =>
            await _chatStreamingMock
                .Object.GetMessageStream(_conversationId, null, null, cancellationTokenSource.Token)
                .ToListAsync(cancellationTokenSource.Token);

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
        var expectedFragments = new List<ClientMessageFragment> { _messageFragments[2] };

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
        var expectedMessageId = Guid.CreateVersion7();

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(prompt.Text))
            .ReturnsAsync(expectedMessageId);

        // Act
        var result = await _chatStreamingMock.Object.AddStreamingMessage(prompt.Text);

        // Assert
        result.ShouldBe(expectedMessageId);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(prompt.Text), Times.Once);
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
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _chatStreamingMock
            .Setup(x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationTokenSource.Token
                )
            )
            .Throws<OperationCanceledException>();

        // Act
        Func<Task> act = async () =>
            await hub.Stream(
                    _conversationId,
                    streamContext,
                    _chatStreamingMock.Object,
                    cancellationTokenSource.Token
                )
                .ToListAsync(cancellationTokenSource.Token);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
        _chatStreamingMock.Verify(
            x =>
                x.GetMessageStream(
                    _conversationId,
                    streamContext.LastMessageId,
                    streamContext.LastFragmentId,
                    cancellationTokenSource.Token
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
}

using BookWorm.Chat.Features.Cancel;
using BookWorm.Chat.Infrastructure.CancellationManager;
using MediatR;

namespace BookWorm.Chat.UnitTests.Features.Cancel;

public sealed class CancelChatCommandTests
{
    private readonly Mock<ICancellationManager> _cancellationManagerMock;
    private readonly Guid _chatId;
    private readonly CancelChatHandler _handler;

    public CancelChatCommandTests()
    {
        _cancellationManagerMock = new();
        _handler = new(_cancellationManagerMock.Object);
        _chatId = Guid.CreateVersion7();
    }

    [Test]
    public async Task GivenValidChatId_WhenCancellingChat_ThenShouldCallCancelAsync()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);

        _cancellationManagerMock.Setup(cm => cm.CancelAsync(_chatId)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public async Task GivenCancellationManagerThrowsException_WhenCancellingChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);
        var expectedException = new InvalidOperationException("Cancellation failed");

        _cancellationManagerMock
            .Setup(cm => cm.CancelAsync(_chatId))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Cancellation failed");
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public async Task GivenCancellationManagerThrowsArgumentException_WhenCancellingChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);
        var expectedException = new ArgumentException("Invalid chat ID", nameof(_chatId));

        _cancellationManagerMock
            .Setup(cm => cm.CancelAsync(_chatId))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldBe("Invalid chat ID (Parameter '_chatId')");
        exception.ParamName.ShouldBe(nameof(_chatId));
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public async Task GivenEmptyGuidChatId_WhenCancellingChat_ThenShouldStillCallCancelAsync()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new CancelChatCommand(emptyGuid);

        _cancellationManagerMock.Setup(cm => cm.CancelAsync(emptyGuid)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(emptyGuid), Times.Once);
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenCancellingChat_ThenShouldIgnoreTokenAndCallCancelAsync()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _cancellationManagerMock.Setup(cm => cm.CancelAsync(_chatId)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldBe(Unit.Value);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public void GivenChatId_WhenCreatingCancelChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Act
        var command = new CancelChatCommand(_chatId);

        // Assert
        command.Id.ShouldBe(_chatId);
    }

    [Test]
    public void GivenTwoCancelChatCommandsWithSameChatId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new CancelChatCommand(_chatId);
        var command2 = new CancelChatCommand(_chatId);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoCancelChatCommandsWithDifferentChatIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var command1 = new CancelChatCommand(chatId1);
        var command2 = new CancelChatCommand(chatId2);

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

    [Test]
    public async Task GivenCancellationManagerReturnsCompleted_WhenCancellingMultipleChats_ThenShouldCallCancelAsyncForEachChat()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var chatId3 = Guid.CreateVersion7();

        var command1 = new CancelChatCommand(chatId1);
        var command2 = new CancelChatCommand(chatId2);
        var command3 = new CancelChatCommand(chatId3);

        _cancellationManagerMock
            .Setup(cm => cm.CancelAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.ShouldBe(Unit.Value);
        result2.ShouldBe(Unit.Value);
        result3.ShouldBe(Unit.Value);

        _cancellationManagerMock.Verify(cm => cm.CancelAsync(chatId1), Times.Once);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(chatId2), Times.Once);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(chatId3), Times.Once);
    }

    [Test]
    public async Task GivenCancellationManagerThrowsRedisException_WhenCancellingChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);
        var expectedException = new InvalidOperationException("Redis connection failed");

        _cancellationManagerMock
            .Setup(cm => cm.CancelAsync(_chatId))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Redis connection failed");
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(_chatId), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingWithCancellationToken_ThenShouldNotThrow()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);
        using var cancellationTokenSource = new CancellationTokenSource();

        _cancellationManagerMock.Setup(cm => cm.CancelAsync(_chatId)).Returns(Task.CompletedTask);

        // Act & Assert
        var result = await _handler.Handle(command, cancellationTokenSource.Token);
        result.ShouldBe(Unit.Value);
    }

    [Test]
    public async Task GivenCancellationManagerCompletes_WhenCancellingChat_ThenShouldReturnUnitValue()
    {
        // Arrange
        var command = new CancelChatCommand(_chatId);

        _cancellationManagerMock.Setup(cm => cm.CancelAsync(_chatId)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
    }

    [Test]
    public async Task GivenHandlerCalledConcurrently_WhenCancellingDifferentChats_ThenShouldHandleEachIndependently()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var command1 = new CancelChatCommand(chatId1);
        var command2 = new CancelChatCommand(chatId2);

        _cancellationManagerMock
            .Setup(cm => cm.CancelAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var task1 = _handler.Handle(command1, CancellationToken.None);
        var task2 = _handler.Handle(command2, CancellationToken.None);
        await Task.WhenAll(task1, task2);

        // Assert
        task1.Result.ShouldBe(Unit.Value);
        task2.Result.ShouldBe(Unit.Value);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(chatId1), Times.Once);
        _cancellationManagerMock.Verify(cm => cm.CancelAsync(chatId2), Times.Once);
    }
}

using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Repository;
using BookWorm.Chat.Domain.AggregatesModel;
using BookWorm.Chat.Features.Delete;
using BookWorm.Chat.UnitTests.Fakers;
using MediatR;

namespace BookWorm.Chat.UnitTests.Features.Delete;

public sealed class DeleteChatCommandTests
{
    private readonly Conversation _conversation;
    private readonly Guid _conversationId;
    private readonly DeleteChatHandler _handler;
    private readonly Mock<IConversationRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public DeleteChatCommandTests()
    {
        _repositoryMock = new();
        _unitOfWorkMock = new();
        _conversation = new ConversationFaker().Generate()[0];
        _conversationId = Guid.CreateVersion7();

        // Set up the conversation ID using reflection since it's read-only
        typeof(Conversation).GetProperty("Id")?.SetValue(_conversation, _conversationId);

        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public void GivenChatId_WhenCreatingDeleteChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new DeleteChatCommand(_conversationId);

        // Assert
        command.Id.ShouldBe(_conversationId);
        command.ShouldBeAssignableTo<ICommand>();
    }

    [Test]
    public void GivenTwoDeleteChatCommandsWithSameChatId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new DeleteChatCommand(_conversationId);
        var command2 = new DeleteChatCommand(_conversationId);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoDeleteChatCommandsWithDifferentChatIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var command1 = new DeleteChatCommand(chatId1);
        var command2 = new DeleteChatCommand(chatId2);

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

    [Test]
    public async Task GivenExistingConversation_WhenHandlingDeleteCommand_ThenShouldDeleteConversationAndSaveChanges()
    {
        // Arrange
        var command = new DeleteChatCommand(_conversationId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_conversation);

        _repositoryMock
            .Setup(r => r.Delete(_conversation, It.IsAny<CancellationToken>()))
            .Returns(true);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(_conversation, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingConversation_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteChatCommand(_conversationId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Conversation with id {_conversationId} not found.");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new DeleteChatCommand(emptyGuid);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Conversation with id {emptyGuid} not found.");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenValidCommandWithCancellationToken_WhenHandlingDeleteCommand_ThenShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var command = new DeleteChatCommand(_conversationId);
        var cancellationToken = new CancellationToken();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_conversationId, cancellationToken))
            .ReturnsAsync(_conversation);

        _repositoryMock.Setup(r => r.Delete(_conversation, cancellationToken)).Returns(true);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(cancellationToken)).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(r => r.GetByIdAsync(_conversationId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.Delete(_conversation, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingDeleteCommand_ThenShouldPropagateException()
    {
        // Arrange
        var command = new DeleteChatCommand(_conversationId);
        var expectedException = new InvalidOperationException("Database error");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Database error");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GivenSaveChangesThrowsException_WhenHandlingDeleteCommand_ThenShouldPropagateException()
    {
        // Arrange
        var command = new DeleteChatCommand(_conversationId);
        var expectedException = new InvalidOperationException("Save failed");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_conversation);

        _repositoryMock
            .Setup(r => r.Delete(_conversation, It.IsAny<CancellationToken>()))
            .Returns(true);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Save failed");

        _repositoryMock.Verify(
            r => r.GetByIdAsync(_conversationId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(_conversation, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingDeleteCommands_ThenShouldProcessEachCommandSeparately()
    {
        // Arrange
        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();
        var conversationId3 = Guid.CreateVersion7();

        var conversation1 = new ConversationFaker().Generate()[0];
        var conversation2 = new ConversationFaker().Generate()[0];
        var conversation3 = new ConversationFaker().Generate()[0];

        // Set up conversation IDs
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation1, conversationId1);
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation2, conversationId2);
        typeof(Conversation).GetProperty("Id")?.SetValue(conversation3, conversationId3);

        var command1 = new DeleteChatCommand(conversationId1);
        var command2 = new DeleteChatCommand(conversationId2);
        var command3 = new DeleteChatCommand(conversationId3);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversationId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation1);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversationId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation2);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(conversationId3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation3);

        _repositoryMock
            .Setup(r => r.Delete(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .Returns(true);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.ShouldBe(Unit.Value);
        result2.ShouldBe(Unit.Value);
        result3.ShouldBe(Unit.Value);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversationId1, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversationId2, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.GetByIdAsync(conversationId3, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _repositoryMock.Verify(
            r => r.Delete(conversation1, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(conversation2, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.Delete(conversation3, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }

    [Test]
    public void GivenDeleteChatHandler_WhenCreating_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var handler = new DeleteChatHandler(_repositoryMock.Object);

        // Assert
        handler.ShouldBeAssignableTo<ICommandHandler<DeleteChatCommand>>();
    }
}

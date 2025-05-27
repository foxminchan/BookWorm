using BookWorm.Chassis.Endpoints;
using BookWorm.Chat.Features.Delete;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.UnitTests.Features.Delete;

public sealed class DeleteChatEndpointTests
{
    private readonly Guid _chatId = Guid.CreateVersion7();
    private readonly DeleteChatEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidChatId_WhenHandlingDeleteChat_ThenShouldSendCommandAndReturnNoContent()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteChatCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(_chatId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == _chatId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidChatIdWithCancellationToken_WhenHandlingDeleteChat_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteChatCommand>(), cancellationToken))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(_chatId, _senderMock.Object, cancellationToken);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s => s.Send(It.Is<DeleteChatCommand>(cmd => cmd.Id == _chatId), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingDeleteChat_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Delete failed");
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteChatCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _endpoint.HandleAsync(_chatId, _senderMock.Object);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Delete failed");
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == _chatId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialRequests_WhenHandlingDeleteChat_ThenShouldHandleEachRequestSeparately()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var chatId3 = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteChatCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result1 = await _endpoint.HandleAsync(chatId1, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(chatId2, _senderMock.Object);
        var result3 = await _endpoint.HandleAsync(chatId3, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<NoContent>();
        result2.ShouldBeOfType<NoContent>();
        result3.ShouldBeOfType<NoContent>();

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == chatId1),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == chatId2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == chatId3),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingDeleteChat_ThenShouldSendCommandWithEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        _senderMock
            .Setup(s => s.Send(It.IsAny<DeleteChatCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(emptyGuid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<DeleteChatCommand>(cmd => cmd.Id == emptyGuid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public void GivenChatId_WhenCreatingDeleteChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new DeleteChatCommand(_chatId);

        // Assert
        command.Id.ShouldBe(_chatId);
    }

    [Test]
    public void GivenTwoDeleteChatCommandsWithSameChatId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new DeleteChatCommand(_chatId);
        var command2 = new DeleteChatCommand(_chatId);

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
    public void GivenDeleteChatEndpoint_WhenCheckingImplementedInterfaces_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var endpoint = new DeleteChatEndpoint();

        // Assert
        endpoint.ShouldBeAssignableTo<IEndpoint<NoContent, Guid, ISender>>();
    }
}

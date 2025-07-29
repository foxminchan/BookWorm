using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Update;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Chat.UnitTests.Features.Update;

public sealed class UpdateChatEndpointTests
{
    private readonly Guid _chatId = Guid.CreateVersion7();
    private readonly UpdateChatEndpoint _endpoint = new();
    private readonly Prompt _prompt = new("What is the best selling book in BookWorm?");
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnNoContent()
    {
        // Arrange
        var command = new UpdateChatCommand(_chatId, _prompt);

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommandWithCancellationToken_WhenHandlingUpdateChat_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        var command = new UpdateChatCommand(_chatId, _prompt);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(command, cancellationToken))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object, cancellationToken);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingUpdateChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new UpdateChatCommand(_chatId, _prompt);
        var expectedException = new InvalidOperationException("Chat service error");

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(command, _senderMock.Object)
        );

        exception.ShouldBe(expectedException);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenEmptyGuidChatId_WhenHandlingUpdateChat_ThenShouldStillCallSender()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new UpdateChatCommand(emptyGuid, _prompt);

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenLongPromptText_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnNoContent()
    {
        // Arrange
        var longText = new string('A', 1000);
        var longPrompt = new Prompt(longText);
        var command = new UpdateChatCommand(_chatId, longPrompt);

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenPromptWithSpecialCharacters_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnNoContent()
    {
        // Arrange
        var specialText = "What about émojis 🚀 and spëcial chars!?";
        var specialPrompt = new Prompt(specialText);
        var command = new UpdateChatCommand(_chatId, specialPrompt);

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<NoContent>();
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingUpdateChat_ThenShouldProcessEachCommandSeparately()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();
        var chatId3 = Guid.CreateVersion7();

        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var prompt3 = new Prompt("Third prompt");

        var command1 = new UpdateChatCommand(chatId1, prompt1);
        var command2 = new UpdateChatCommand(chatId2, prompt2);
        var command3 = new UpdateChatCommand(chatId3, prompt3);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateChatCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var result1 = await _endpoint.HandleAsync(command1, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(command2, _senderMock.Object);
        var result3 = await _endpoint.HandleAsync(command3, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<NoContent>();
        result2.ShouldBeOfType<NoContent>();
        result3.ShouldBeOfType<NoContent>();

        _senderMock.Verify(s => s.Send(command1, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command2, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenConcurrentRequests_WhenHandlingUpdateChat_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var chatId1 = Guid.CreateVersion7();
        var chatId2 = Guid.CreateVersion7();

        var prompt1 = new Prompt("Concurrent prompt 1");
        var prompt2 = new Prompt("Concurrent prompt 2");

        var command1 = new UpdateChatCommand(chatId1, prompt1);
        var command2 = new UpdateChatCommand(chatId2, prompt2);

        _senderMock
            .Setup(s => s.Send(It.IsAny<UpdateChatCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Unit.Value));

        // Act
        var task1 = _endpoint.HandleAsync(command1, _senderMock.Object);
        var task2 = _endpoint.HandleAsync(command2, _senderMock.Object);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results[0].ShouldBeOfType<NoContent>();
        results[1].ShouldBeOfType<NoContent>();

        _senderMock.Verify(s => s.Send(command1, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GivenUpdateChatEndpoint_WhenCreating_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var endpoint = new UpdateChatEndpoint();

        // Assert
        endpoint.ShouldBeAssignableTo<IEndpoint<NoContent, UpdateChatCommand, ISender>>();
    }

    [Test]
    public void GivenChatIdAndPrompt_WhenCreatingUpdateChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new UpdateChatCommand(_chatId, _prompt);

        // Assert
        command.Id.ShouldBe(_chatId);
        command.Prompt.ShouldBe(_prompt);
        command.Prompt.Text.ShouldBe("What is the best selling book in BookWorm?");
        command.ShouldBeAssignableTo<ICommand>();
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new UpdateChatCommand(_chatId, _prompt);
        var command2 = new UpdateChatCommand(_chatId, _prompt);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentChatIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var differentChatId = Guid.CreateVersion7();
        var command1 = new UpdateChatCommand(_chatId, _prompt);
        var command2 = new UpdateChatCommand(differentChatId, _prompt);

        // Act & Assert
        command1.ShouldNotBe(command2);
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentPrompts_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var differentPrompt = new Prompt("Different prompt text");
        var command1 = new UpdateChatCommand(_chatId, _prompt);
        var command2 = new UpdateChatCommand(_chatId, differentPrompt);

        // Act & Assert
        command1.ShouldNotBe(command2);
    }
}

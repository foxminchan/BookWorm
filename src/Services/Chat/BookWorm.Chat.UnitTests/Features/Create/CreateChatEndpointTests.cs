using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Create;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace BookWorm.Chat.UnitTests.Features.Create;

public sealed class CreateChatEndpointTests
{
    private readonly CreateChatEndpoint _endpoint = new();
    private readonly LinkGenerator _linkGenerator = new Mock<LinkGenerator>().Object;
    private readonly Prompt _prompt = new("What is the best selling book in BookWorm?");
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnCreated()
    {
        // Arrange
        var command = new CreateChatCommand(_prompt);
        var conversationId = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(conversationId);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommandWithCancellationToken_WhenHandlingUpdateChat_ThenShouldPassCancellationTokenToSender()
    {
        // Arrange
        var command = new CreateChatCommand(_prompt);
        var conversationId = Guid.CreateVersion7();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock.Setup(s => s.Send(command, cancellationToken)).ReturnsAsync(conversationId);

        // Act
        var result = await _endpoint.HandleAsync(
            command,
            _senderMock.Object,
            _linkGenerator,
            cancellationToken
        );

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(conversationId);
        _senderMock.Verify(s => s.Send(command, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingUpdateChat_ThenShouldPropagateException()
    {
        // Arrange
        var command = new CreateChatCommand(_prompt);
        var expectedException = new InvalidOperationException("Chat service error");

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(command, _senderMock.Object, _linkGenerator)
        );

        exception.ShouldBe(expectedException);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateChat_ThenShouldReturnValidGuid()
    {
        // Arrange
        var command = new CreateChatCommand(_prompt);
        var conversationId = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldNotBe(Guid.Empty);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenLongPromptText_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnCreated()
    {
        // Arrange
        var longText = new string('A', 1000);
        var longPrompt = new Prompt(longText);
        var command = new CreateChatCommand(longPrompt);
        var conversationId = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(conversationId);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenPromptWithSpecialCharacters_WhenHandlingUpdateChat_ThenShouldCallSenderAndReturnCreated()
    {
        // Arrange
        const string specialText = "What about émojis 🚀 and spëcial chars!?";
        var specialPrompt = new Prompt(specialText);
        var command = new CreateChatCommand(specialPrompt);
        var conversationId = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId);

        // Act
        var result = await _endpoint.HandleAsync(command, _senderMock.Object, _linkGenerator);

        // Assert
        result.ShouldBeOfType<Created<Guid>>();
        result.Value.ShouldBe(conversationId);
        _senderMock.Verify(s => s.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingUpdateChat_ThenShouldProcessEachCommandSeparately()
    {
        // Arrange
        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var prompt3 = new Prompt("Third prompt");

        var command1 = new CreateChatCommand(prompt1);
        var command2 = new CreateChatCommand(prompt2);
        var command3 = new CreateChatCommand(prompt3);

        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();
        var conversationId3 = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId1);
        _senderMock
            .Setup(s => s.Send(command2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId2);
        _senderMock
            .Setup(s => s.Send(command3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId3);

        // Act
        var result1 = await _endpoint.HandleAsync(command1, _senderMock.Object, _linkGenerator);
        var result2 = await _endpoint.HandleAsync(command2, _senderMock.Object, _linkGenerator);
        var result3 = await _endpoint.HandleAsync(command3, _senderMock.Object, _linkGenerator);

        // Assert
        result1.ShouldBeOfType<Created<Guid>>();
        result1.Value.ShouldBe(conversationId1);
        result2.ShouldBeOfType<Created<Guid>>();
        result2.Value.ShouldBe(conversationId2);
        result3.ShouldBeOfType<Created<Guid>>();
        result3.Value.ShouldBe(conversationId3);

        _senderMock.Verify(s => s.Send(command1, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command2, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenConcurrentRequests_WhenHandlingUpdateChat_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var prompt1 = new Prompt("Concurrent prompt 1");
        var prompt2 = new Prompt("Concurrent prompt 2");

        var command1 = new CreateChatCommand(prompt1);
        var command2 = new CreateChatCommand(prompt2);

        var conversationId1 = Guid.CreateVersion7();
        var conversationId2 = Guid.CreateVersion7();

        _senderMock
            .Setup(s => s.Send(command1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId1);
        _senderMock
            .Setup(s => s.Send(command2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversationId2);

        // Act
        var task1 = _endpoint.HandleAsync(command1, _senderMock.Object, _linkGenerator);
        var task2 = _endpoint.HandleAsync(command2, _senderMock.Object, _linkGenerator);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        results[0].ShouldBeOfType<Created<Guid>>();
        results[0].Value.ShouldBe(conversationId1);
        results[1].ShouldBeOfType<Created<Guid>>();
        results[1].Value.ShouldBe(conversationId2);

        _senderMock.Verify(s => s.Send(command1, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(command2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GivenUpdateChatEndpoint_WhenCreating_ThenShouldImplementCorrectInterface()
    {
        // Arrange & Act
        var endpoint = new CreateChatEndpoint();

        // Assert
        endpoint.ShouldBeAssignableTo<
            IEndpoint<Created<Guid>, CreateChatCommand, ISender, LinkGenerator>
        >();
    }

    [Test]
    public void GivenChatIdAndPrompt_WhenCreatingUpdateChatCommand_ThenPropertiesShouldBeCorrectlyInitialized()
    {
        // Arrange & Act
        var command = new CreateChatCommand(_prompt);

        // Assert
        command.Prompt.ShouldBe(_prompt);
        command.Prompt.Text.ShouldBe("What is the best selling book in BookWorm?");
        command.ShouldBeAssignableTo<ICommand<Guid>>();
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var command1 = new CreateChatCommand(_prompt);
        var command2 = new CreateChatCommand(_prompt);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentPrompts_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var differentPrompt = new Prompt("Different prompt text");
        var command1 = new CreateChatCommand(_prompt);
        var command2 = new CreateChatCommand(differentPrompt);

        // Act & Assert
        command1.ShouldNotBe(command2);
    }
}

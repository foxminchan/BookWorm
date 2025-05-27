using BookWorm.Chassis.Command;
using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Update;
using BookWorm.Chat.Infrastructure.ChatStreaming;
using MediatR;

namespace BookWorm.Chat.UnitTests.Features.Update;

public sealed class UpdateChatCommandTests
{
    private readonly Mock<IChatStreaming> _chatStreamingMock = new();
    private readonly UpdateChatHandler _handler;

    public UpdateChatCommandTests()
    {
        _handler = new(_chatStreamingMock.Object);
    }

    [Test]
    public void GivenUpdateChatCommand_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("Test prompt");

        // Act
        var command = new UpdateChatCommand(id, prompt);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<UpdateChatCommand>();
        command.ShouldBeAssignableTo<ICommand>();
        command.Id.ShouldBe(id);
        command.Prompt.ShouldBe(prompt);
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("Test prompt");
        var command1 = new UpdateChatCommand(id, prompt);
        var command2 = new UpdateChatCommand(id, prompt);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
        (command1 == command2).ShouldBeTrue();
        (command1 != command2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();
        var prompt = new Prompt("Test prompt");
        var command1 = new UpdateChatCommand(id1, prompt);
        var command2 = new UpdateChatCommand(id2, prompt);

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
        (command1 == command2).ShouldBeFalse();
        (command1 != command2).ShouldBeTrue();
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentPrompts_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var command1 = new UpdateChatCommand(id, prompt1);
        var command2 = new UpdateChatCommand(id, prompt2);

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
        (command1 == command2).ShouldBeFalse();
        (command1 != command2).ShouldBeTrue();
    }

    [Test]
    public void GivenUpdateChatHandler_WhenCheckingImplementedInterfaces_ThenShouldImplementCorrectInterface()
    {
        // Act & Assert
        _handler.ShouldNotBeNull();
        _handler.ShouldBeOfType<UpdateChatHandler>();
        _handler.ShouldBeAssignableTo<ICommandHandler<UpdateChatCommand>>();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessage()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("What is the weather today?");
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, prompt.Text), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithEmptyPromptText_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithEmptyText()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("");
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, ""), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithWhitespacePromptText_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithWhitespace()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("   ");
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, "   "), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithComplexPrompt_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithCompleteText()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string complexPrompt =
            "Can you explain the difference between machine learning and artificial intelligence? Please provide examples and use cases for each technology.";
        var prompt = new Prompt(complexPrompt);
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, complexPrompt), Times.Once);
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenHandlingUpdateChat_ThenShouldPassTokenToChatStreaming()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("Test prompt");
        var command = new UpdateChatCommand(id, prompt);
        var cancellationToken = new CancellationToken(true);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, prompt.Text), Times.Once);
    }

    [Test]
    public async Task GivenChatStreamingThrowsException_WhenHandlingUpdateChat_ThenShouldPropagateException()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("Test prompt");
        var command = new UpdateChatCommand(id, prompt);
        var expectedException = new InvalidOperationException("Chat streaming service unavailable");

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Chat streaming service unavailable");
        exception.ShouldBe(expectedException);

        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, prompt.Text), Times.Once);
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingUpdateChat_ThenShouldHandleEachCommandIndependently()
    {
        // Arrange
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();
        var id3 = Guid.CreateVersion7();
        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var prompt3 = new Prompt("Third prompt");
        var command1 = new UpdateChatCommand(id1, prompt1);
        var command2 = new UpdateChatCommand(id2, prompt2);
        var command3 = new UpdateChatCommand(id3, prompt3);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.ShouldBe(Unit.Value);
        result2.ShouldBe(Unit.Value);
        result3.ShouldBe(Unit.Value);

        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id1, prompt1.Text), Times.Once);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id2, prompt2.Text), Times.Once);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id3, prompt3.Text), Times.Once);
    }

    [Test]
    public async Task GivenSameCommandExecutedMultipleTimes_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageEachTime()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var prompt = new Prompt("Repeated prompt");
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);
        var result3 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.ShouldBe(Unit.Value);
        result2.ShouldBe(Unit.Value);
        result3.ShouldBe(Unit.Value);

        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, prompt.Text), Times.Exactly(3));
    }

    [Test]
    public async Task GivenCommandWithSpecialCharactersInPrompt_WhenHandlingUpdateChat_ThenShouldHandleSpecialCharacters()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        const string specialPrompt = "Hello! @#$%^&*()_+-=[]{}|;':\",./<>? 🚀🔥💡";
        var prompt = new Prompt(specialPrompt);
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, specialPrompt), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithMultilinePrompt_WhenHandlingUpdateChat_ThenShouldPreserveFormatting()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var multilinePrompt = """
            This is a multiline prompt.

            It contains:
            - Line breaks
            - Indentation
            - Multiple paragraphs

            Please process this correctly.
            """;
        var prompt = new Prompt(multilinePrompt);
        var command = new UpdateChatCommand(id, prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(id, prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(id, multilinePrompt), Times.Once);
    }
}

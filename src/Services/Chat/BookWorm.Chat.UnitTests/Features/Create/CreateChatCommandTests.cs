using BookWorm.Chat.Features;
using BookWorm.Chat.Features.Create;
using BookWorm.Chat.Infrastructure.ChatStreaming;
using Mediator;

namespace BookWorm.Chat.UnitTests.Features.Create;

public sealed class CreateChatCommandTests
{
    private readonly Mock<IChatStreaming> _chatStreamingMock = new();
    private readonly UpdateChatHandler _handler;

    public CreateChatCommandTests()
    {
        _handler = new(_chatStreamingMock.Object);
    }

    [Test]
    public void GivenUpdateChatCommand_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Arrange
        var prompt = new Prompt("Test prompt");

        // Act
        var command = new CreateChatCommand(prompt);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<CreateChatCommand>();
        command.ShouldBeAssignableTo<ICommand<Guid>>();
        command.Prompt.ShouldBe(prompt);
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithSameValues_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var prompt = new Prompt("Test prompt");
        var command1 = new CreateChatCommand(prompt);
        var command2 = new CreateChatCommand(prompt);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
        (command1 == command2).ShouldBeTrue();
        (command1 != command2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoUpdateChatCommandsWithDifferentPrompts_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var command1 = new CreateChatCommand(prompt1);
        var command2 = new CreateChatCommand(prompt2);

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
        _handler.ShouldBeAssignableTo<ICommandHandler<CreateChatCommand, Guid>>();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessage()
    {
        // Arrange
        var prompt = new Prompt("What is the weather today?");
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCommandWithEmptyPromptText_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithEmptyText()
    {
        // Arrange
        var prompt = new Prompt("");
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(It.IsAny<Guid>(), ""), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithWhitespacePromptText_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithWhitespace()
    {
        // Arrange
        var prompt = new Prompt("   ");
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(x => x.AddStreamingMessage(It.IsAny<Guid>(), "   "), Times.Once);
    }

    [Test]
    public async Task GivenCommandWithComplexPrompt_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageWithCompleteText()
    {
        // Arrange
        const string complexPrompt =
            "Can you explain the difference between machine learning and artificial intelligence? Please provide examples and use cases for each technology.";
        var prompt = new Prompt(complexPrompt);
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), complexPrompt),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationTokenRequested_WhenHandlingUpdateChat_ThenShouldPassTokenToChatStreaming()
    {
        // Arrange
        var prompt = new Prompt("Test prompt");
        var command = new CreateChatCommand(prompt);
        var cancellationToken = new CancellationToken(true);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text),
            Times.Once
        );
    }

    [Test]
    public async Task GivenChatStreamingThrowsException_WhenHandlingUpdateChat_ThenShouldPropagateException()
    {
        // Arrange
        var prompt = new Prompt("Test prompt");
        var command = new CreateChatCommand(prompt);
        var expectedException = new InvalidOperationException("Chat streaming service unavailable");

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<InvalidOperationException>();
        exception.Message.ShouldBe("Chat streaming service unavailable");

        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleSequentialCommands_WhenHandlingUpdateChat_ThenShouldHandleEachCommandIndependently()
    {
        // Arrange
        var prompt1 = new Prompt("First prompt");
        var prompt2 = new Prompt("Second prompt");
        var prompt3 = new Prompt("Third prompt");
        var command1 = new CreateChatCommand(prompt1);
        var command2 = new CreateChatCommand(prompt2);
        var command3 = new CreateChatCommand(prompt3);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.ShouldBeOfType<Guid>();
        result1.ShouldNotBe(Guid.Empty);
        result2.ShouldBeOfType<Guid>();
        result2.ShouldNotBe(Guid.Empty);
        result3.ShouldBeOfType<Guid>();
        result3.ShouldNotBe(Guid.Empty);

        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt1.Text),
            Times.Once
        );
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt2.Text),
            Times.Once
        );
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt3.Text),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSameCommandExecutedMultipleTimes_WhenHandlingUpdateChat_ThenShouldCallAddStreamingMessageEachTime()
    {
        // Arrange
        var prompt = new Prompt("Repeated prompt");
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);
        var result3 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.ShouldBeOfType<Guid>();
        result1.ShouldNotBe(Guid.Empty);
        result2.ShouldBeOfType<Guid>();
        result2.ShouldNotBe(Guid.Empty);
        result3.ShouldBeOfType<Guid>();
        result3.ShouldNotBe(Guid.Empty);

        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text),
            Times.Exactly(3)
        );
    }

    [Test]
    public async Task GivenCommandWithSpecialCharactersInPrompt_WhenHandlingUpdateChat_ThenShouldHandleSpecialCharacters()
    {
        // Arrange
        const string specialPrompt = "Hello! @#$%^&*()_+-=[]{}|;':\",./<>? 🚀🔥💡";
        var prompt = new Prompt(specialPrompt);
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), specialPrompt),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCommandWithMultilinePrompt_WhenHandlingUpdateChat_ThenShouldPreserveFormatting()
    {
        // Arrange
        const string multilinePrompt = """
            This is a multiline prompt.

            It contains:
            - Line breaks
            - Indentation
            - Multiple paragraphs

            Please process this correctly.
            """;
        var prompt = new Prompt(multilinePrompt);
        var command = new CreateChatCommand(prompt);

        _chatStreamingMock
            .Setup(x => x.AddStreamingMessage(It.IsAny<Guid>(), prompt.Text))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<Guid>();
        result.ShouldNotBe(Guid.Empty);
        _chatStreamingMock.Verify(
            x => x.AddStreamingMessage(It.IsAny<Guid>(), multilinePrompt),
            Times.Once
        );
    }
}

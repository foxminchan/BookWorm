using BookWorm.Chat.Orchestration.Executors;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Executors;

public sealed class ResponseFormatterExecutorTests
{
    private readonly ResponseFormatterExecutor _sut = new();
    private readonly IWorkflowContext _context = Mock.Of<IWorkflowContext>();

    [Test]
    public void GivenNullMessages_WhenHandling_ThenShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await _sut.HandleAsync(null!, _context));
    }

    [Test]
    public async Task GivenEmptyMessagesList_WhenHandling_ThenShouldReturnFallbackMessage()
    {
        // Act
        var result = await _sut.HandleAsync([], _context);

        // Assert
        result.ShouldContain("couldn't generate a proper response");
    }

    [Test]
    public async Task GivenNoAssistantMessages_WhenHandling_ThenShouldReturnFallbackMessage()
    {
        // Arrange
        List<ChatMessage> messages =
        [
            new(ChatRole.User, "Hello"),
            new(ChatRole.User, "Can you help?"),
        ];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldContain("couldn't generate a proper response");
    }

    [Test]
    public async Task GivenAssistantMessageWithWhitespaceOnly_WhenHandling_ThenShouldReturnFallback()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "   ")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldContain("couldn't generate a proper response");
    }

    [Test]
    public async Task GivenMessageEndingWithPeriod_WhenHandling_ThenShouldNotAddPeriod()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "Here is your book.")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Here is your book.");
    }

    [Test]
    [Arguments("Here is your book!")]
    [Arguments("Is this what you need?")]
    [Arguments("She said \"hello\"")]
    [Arguments("(see details above)")]
    public async Task GivenMessageEndingWithValidPunctuation_WhenHandling_ThenShouldNotAddPeriod(
        string text
    )
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, text)];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe(text);
    }

    [Test]
    public async Task GivenMessageMissingEndingPunctuation_WhenHandling_ThenShouldAddPeriod()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "Here is your book")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Here is your book.");
    }

    [Test]
    public async Task GivenMessageWithExcessiveNewlines_WhenHandling_ThenShouldReduceToDouble()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "Line one.\n\n\n\n\nLine two.")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Line one.\n\nLine two.");
    }

    [Test]
    public async Task GivenMessageWithDoubleNewlines_WhenHandling_ThenShouldPreserve()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "Line one.\n\nLine two.")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Line one.\n\nLine two.");
    }

    [Test]
    public async Task GivenMessageWithLeadingTrailingSpaces_WhenHandling_ThenShouldTrim()
    {
        // Arrange
        List<ChatMessage> messages = [new(ChatRole.Assistant, "  Hello there.  ")];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Hello there.");
    }

    [Test]
    public async Task GivenMultipleAssistantMessages_WhenHandling_ThenShouldUseLastAssistantMessage()
    {
        // Arrange
        List<ChatMessage> messages =
        [
            new(ChatRole.Assistant, "First response."),
            new(ChatRole.User, "Thanks, but can you elaborate?"),
            new(ChatRole.Assistant, "Here is a more detailed answer"),
        ];

        // Act
        var result = await _sut.HandleAsync(messages, _context);

        // Assert
        result.ShouldBe("Here is a more detailed answer.");
    }
}

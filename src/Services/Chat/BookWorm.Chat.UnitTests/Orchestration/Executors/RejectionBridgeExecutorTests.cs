using BookWorm.Chat.Orchestration.Executors;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Executors;

public sealed class RejectionBridgeExecutorTests
{
    private readonly RejectionBridgeExecutor _sut = new();
    private readonly IWorkflowContext _context = Mock.Of<IWorkflowContext>();

    [Test]
    public void GivenNullMessage_WhenHandling_ThenShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await _sut.HandleAsync(null!, _context));
    }

    [Test]
    public async Task GivenAssistantRejection_WhenHandling_ThenShouldReturnSingleItemList()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.Assistant, "Request blocked.");

        // Act
        var result = await _sut.HandleAsync(message, _context);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].ShouldBeSameAs(message);
    }

    [Test]
    public async Task GivenAssistantRejection_WhenHandling_ThenShouldPreserveRoleAndContent()
    {
        // Arrange
        const string text = "I'm sorry, but I can't process that request.";
        var message = new ChatMessage(ChatRole.Assistant, text);

        // Act
        var result = await _sut.HandleAsync(message, _context);

        // Assert
        result[0].Role.ShouldBe(ChatRole.Assistant);
        result[0].Text.ShouldBe(text);
    }

    [Test]
    public async Task GivenUserMessage_WhenHandling_ThenShouldStillWrapInList()
    {
        // Arrange
        var message = new ChatMessage(ChatRole.User, "Hello");

        // Act
        var result = await _sut.HandleAsync(message, _context);

        // Assert
        result.Count.ShouldBe(1);
        result[0].ShouldBeSameAs(message);
    }
}

using BookWorm.Chassis.AI.Decisions;
using BookWorm.Chat.Domain.Services;
using BookWorm.Chat.Orchestration;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration;

public sealed class WorkflowRegistrationTests
{
    [Test]
    public async Task GivenNonEnglishDecision_WhenRunningWorkflow_ThenShortCircuitsWithoutCallingAgents()
    {
        // Arrange
        var bookCalled = false;
        var qaCalled = false;
        var ratingCalled = false;

        var bookAgent = CreateStubAgent("book", () => bookCalled = true, "book-reply");
        var qaAgent = CreateStubAgent("qa", () => qaCalled = true, "qa-reply");
        var ratingAgent = CreateStubAgent("rating", () => ratingCalled = true, "rating-reply");

        var decisionClient = new Mock<IDecisionClient>();
        decisionClient
            .Setup(c =>
                c.GetDecisionAsync(
                    It.IsAny<DecisionRequest>(),
                    It.IsAny<DecisionOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DecisionResponse([
                    new BinaryDecisionAnswer("is_english", 0.02),
                    new BinaryDecisionAnswer("is_in_scope", 0.90),
                ])
            );

        var workflow = WorkflowRegistration.BuildChatWorkflow(
            bookAgent,
            qaAgent,
            ratingAgent,
            new PromptValidationService(),
            decisionClient.Object
        );

        // Act
        await using var run = await InProcessExecution.RunAsync(
            workflow,
            new ChatMessage(ChatRole.User, "Tôi muốn mua sách")
        );
        var outputs = run
            .OutgoingEvents.OfType<WorkflowOutputEvent>()
            .Select(e => e.Data?.ToString())
            .ToList();

        // Assert
        outputs.ShouldContain(o => o != null && o.Contains("English only"));
        bookCalled.ShouldBeFalse();
        qaCalled.ShouldBeFalse();
        ratingCalled.ShouldBeFalse();
    }

    [Test]
    [Arguments(BookWorm.Constants.Other.Agents.BookAgent, "book-reply")]
    [Arguments(BookWorm.Constants.Other.Agents.QAAgent, "qa-reply")]
    [Arguments(BookWorm.Constants.Other.Agents.RatingAgent, "rating-reply")]
    public async Task GivenRoutedDecision_WhenRunningWorkflow_ThenInvokesSelectedAgent(
        string selectedCandidateId,
        string expectedReply
    )
    {
        // Arrange
        var bookAgent = CreateStubAgent("book", null, "book-reply");
        var qaAgent = CreateStubAgent("qa", null, "qa-reply");
        var ratingAgent = CreateStubAgent("rating", null, "rating-reply");

        var decisionClient = new Mock<IDecisionClient>();
        decisionClient
            .Setup(c =>
                c.GetDecisionAsync(
                    It.IsAny<DecisionRequest>(),
                    It.IsAny<DecisionOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DecisionResponse([
                    new BinaryDecisionAnswer("is_english", 0.99),
                    new BinaryDecisionAnswer("is_in_scope", 0.95),
                    new ChoiceDecisionAnswer(
                        "route",
                        selectedCandidateId,
                        0.93,
                        [new DecisionProbability(selectedCandidateId, 0.93)]
                    ),
                ])
            );

        var workflow = WorkflowRegistration.BuildChatWorkflow(
            bookAgent,
            qaAgent,
            ratingAgent,
            new PromptValidationService(),
            decisionClient.Object
        );

        // Act
        await using var run = await InProcessExecution.RunAsync(
            workflow,
            new ChatMessage(ChatRole.User, "Help with BookWorm")
        );
        var outputs = run
            .OutgoingEvents.OfType<WorkflowOutputEvent>()
            .Select(e => e.Data?.ToString())
            .ToList();

        // Assert
        var debugEvents = string.Join(
            " | ",
            run.OutgoingEvents.Select(e => $"{e.GetType().Name}:{e}")
        );
        outputs.ShouldContain(o => o != null && o.Contains(expectedReply), debugEvents);
    }

    [Test]
    public async Task GivenOutOfScopeDecision_WhenRunningWorkflow_ThenShortCircuitsWithoutCallingAgents()
    {
        // Arrange
        var bookCalled = false;
        var qaCalled = false;
        var ratingCalled = false;

        var bookAgent = CreateStubAgent("book", () => bookCalled = true, "book-reply");
        var qaAgent = CreateStubAgent("qa", () => qaCalled = true, "qa-reply");
        var ratingAgent = CreateStubAgent("rating", () => ratingCalled = true, "rating-reply");

        var decisionClient = new Mock<IDecisionClient>();
        decisionClient
            .Setup(c =>
                c.GetDecisionAsync(
                    It.IsAny<DecisionRequest>(),
                    It.IsAny<DecisionOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DecisionResponse([
                    new BinaryDecisionAnswer("is_english", 0.99),
                    new BinaryDecisionAnswer("is_in_scope", 0.05),
                ])
            );

        var workflow = WorkflowRegistration.BuildChatWorkflow(
            bookAgent,
            qaAgent,
            ratingAgent,
            new PromptValidationService(),
            decisionClient.Object
        );

        // Act
        await using var run = await InProcessExecution.RunAsync(
            workflow,
            new ChatMessage(ChatRole.User, "What is the weather today?")
        );
        var outputs = run
            .OutgoingEvents.OfType<WorkflowOutputEvent>()
            .Select(e => e.Data?.ToString())
            .ToList();

        // Assert
        outputs.ShouldContain(o => o != null && o.Contains("BookWorm bookstore topics"));
        bookCalled.ShouldBeFalse();
        qaCalled.ShouldBeFalse();
        ratingCalled.ShouldBeFalse();
    }

    private static ChatClientAgent CreateStubAgent(string name, Action? onInvoked, string replyText)
    {
        var chatClient = new Mock<IChatClient>();
        chatClient
            .Setup(c =>
                c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                (_, _, _) =>
                {
                    onInvoked?.Invoke();
                    return Task.FromResult(
                        new ChatResponse(new ChatMessage(ChatRole.Assistant, replyText))
                    );
                }
            );

        chatClient
            .Setup(c =>
                c.GetStreamingResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                (_, _, _) =>
                {
                    onInvoked?.Invoke();
                    return StreamReply(replyText);
                }
            );

        return new ChatClientAgent(chatClient.Object, name: name);

        static async IAsyncEnumerable<ChatResponseUpdate> StreamReply(string text)
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, text);
            await Task.CompletedTask;
        }
    }
}

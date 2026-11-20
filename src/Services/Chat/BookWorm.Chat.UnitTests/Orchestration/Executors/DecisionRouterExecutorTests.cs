using BookWorm.Chassis.AI.Decisions;
using BookWorm.Chat.Infrastructure.Decisions;
using BookWorm.Chat.Orchestration.Executors;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Executors;

public sealed class DecisionRouterExecutorTests
{
    private readonly IWorkflowContext _context = Mock.Of<IWorkflowContext>();
    private readonly Mock<IDecisionClient> _decisionClient = new();
    private readonly DecisionRouterExecutor _sut;

    public DecisionRouterExecutorTests()
    {
        _sut = new DecisionRouterExecutor(_decisionClient.Object);
    }

    [Test]
    public void GivenNullMessage_WhenHandling_ThenShouldThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(async () => await _sut.HandleAsync(null!, _context));
    }

    [Test]
    public async Task GivenNonEnglishMessage_WhenHandling_ThenShouldRouteToNonEnglish()
    {
        // Arrange
        SetupDecision(
            isEnglish: 0.05,
            isInScope: 0.90,
            selectedRoute: BookWorm.Constants.Other.Agents.BookAgent
        );
        var message = new ChatMessage(ChatRole.User, "Tôi muốn mua sách");

        // Act
        var decision = await _sut.HandleAsync(message, _context);

        // Assert
        decision.Target.ShouldBe(RouteTarget.NonEnglish);
        decision.RejectionReason.ShouldBe(ChatRoutingDecisionService.NonEnglishMessage);
        decision.Message.ShouldBe(message);
    }

    [Test]
    public async Task GivenOutOfScopeMessage_WhenHandling_ThenShouldRouteToOutOfScope()
    {
        // Arrange
        SetupDecision(
            isEnglish: 0.99,
            isInScope: 0.08,
            selectedRoute: BookWorm.Constants.Other.Agents.BookAgent
        );
        var message = new ChatMessage(ChatRole.User, "What is the weather today?");

        // Act
        var decision = await _sut.HandleAsync(message, _context);

        // Assert
        decision.Target.ShouldBe(RouteTarget.OutOfScope);
        decision.RejectionReason.ShouldBe(ChatRoutingDecisionService.OutOfScopeMessage);
    }

    [Test]
    [Arguments(BookWorm.Constants.Other.Agents.BookAgent, nameof(RouteTarget.Book))]
    [Arguments(BookWorm.Constants.Other.Agents.QAAgent, nameof(RouteTarget.QA))]
    [Arguments(BookWorm.Constants.Other.Agents.RatingAgent, nameof(RouteTarget.Rating))]
    public async Task GivenInScopeEnglishMessage_WhenHandling_ThenShouldRouteToSelectedAgent(
        string candidateId,
        string expectedTargetName
    )
    {
        // Arrange
        var expectedTarget = Enum.Parse<RouteTarget>(expectedTargetName);
        SetupDecision(
            isEnglish: 0.99,
            isInScope: 0.95,
            selectedRoute: candidateId,
            confidence: 0.91
        );
        var message = new ChatMessage(ChatRole.User, "Help me with BookWorm");

        // Act
        var decision = await _sut.HandleAsync(message, _context);

        // Assert
        decision.Target.ShouldBe(expectedTarget);
        decision.RejectionReason.ShouldBeNull();
        decision.Confidence.ShouldBe(0.91);
        decision.Message.ShouldBe(message);
    }

    [Test]
    public async Task GivenUnansweredRoute_WhenHandling_ThenShouldDefaultToBookAgent()
    {
        // Arrange
        _decisionClient
            .Setup(c =>
                c.GetDecisionAsync(
                    It.IsAny<DecisionRequest>(),
                    It.IsAny<DecisionOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DecisionResponse([
                    new BinaryDecisionAnswer("is_english", 0.98),
                    new BinaryDecisionAnswer("is_in_scope", 0.92),
                    new UnansweredDecisionAnswer(
                        "route",
                        DecisionKind.Choice,
                        DecisionUnansweredReason.Refusal
                    ),
                ])
            );

        var message = new ChatMessage(ChatRole.User, "Show me sci-fi books");

        // Act
        var decision = await _sut.HandleAsync(message, _context);

        // Assert
        decision.Target.ShouldBe(RouteTarget.Book);
        decision.RejectionReason.ShouldBeNull();
    }

    private void SetupDecision(
        double isEnglish,
        double isInScope,
        string selectedRoute,
        double confidence = 0.9
    )
    {
        _decisionClient
            .Setup(c =>
                c.GetDecisionAsync(
                    It.IsAny<DecisionRequest>(),
                    It.IsAny<DecisionOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new DecisionResponse([
                    new BinaryDecisionAnswer("is_english", isEnglish),
                    new BinaryDecisionAnswer("is_in_scope", isInScope),
                    new ChoiceDecisionAnswer(
                        "route",
                        selectedRoute,
                        [new DecisionProbability(selectedRoute, confidence)]
                    ),
                ])
            );
    }
}

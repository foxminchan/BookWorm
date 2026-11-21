using BookWorm.Chat.Orchestration;
using BookWorm.Chat.Orchestration.Executors.Disambiguation;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration;

public sealed class AGUIWorkflowAgentTests
{
    [Test]
    public async Task GivenRunAsync_WhenInvoked_ThenPushesContextAndCallsInnerAgent()
    {
        // Arrange
        AGUIRunState? observedState = null;
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
                    observedState = AGUIRunContext.Current;
                    return Task.FromResult(
                        new ChatResponse(new ChatMessage(ChatRole.Assistant, "test-response"))
                    );
                }
            );

        var innerAgent = new ChatClientAgent(chatClient.Object);
        var workflowAgent = new AGUIWorkflowAgent(innerAgent);
        var options = new ChatClientAgentRunOptions(new());

        // Act
        var response = await workflowAgent.RunAsync([new(ChatRole.User, "hi")], null, options);

        // Assert
        response.ShouldNotBeNull();
        response.Text.ShouldBe("test-response");
        observedState.ShouldNotBeNull();
        AGUIRunContext.Current.ShouldBeNull();
    }

    [Test]
    public async Task GivenRunStreamingAsync_WhenInvoked_ThenPushesContextAndYieldsInnerUpdates()
    {
        // Arrange
        AGUIRunState? observedState = null;
        var chatClient = new Mock<IChatClient>();
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
                    observedState = AGUIRunContext.Current;
                    return StreamUpdates();
                }
            );

        static async IAsyncEnumerable<ChatResponseUpdate> StreamUpdates()
        {
            yield return new(ChatRole.Assistant, "chunk-1");
            yield return new(ChatRole.Assistant, "chunk-2");
            await Task.CompletedTask;
        }

        var innerAgent = new ChatClientAgent(chatClient.Object);
        var workflowAgent = new AGUIWorkflowAgent(innerAgent);
        var options = new ChatClientAgentRunOptions(new());

        // Act
        var updates = new List<AgentResponseUpdate>();
        await foreach (
            var update in workflowAgent.RunStreamingAsync(
                [new(ChatRole.User, "stream")],
                null,
                options
            )
        )
        {
            updates.Add(update);
        }

        // Assert
        updates.Count.ShouldBeGreaterThan(0);
        observedState.ShouldNotBeNull();
        AGUIRunContext.Current.ShouldBeNull();
    }

    [Test]
    public void GivenUnansweredDisambiguationCall_WhenUserRepliesWithText_ThenNormalizesToToolResult()
    {
        // Arrange
        ChatMessage[] messages =
        [
            new(ChatRole.User, "Help with my order"),
            new(
                ChatRole.Assistant,
                [new FunctionCallContent("call-123", RouteDisambiguationConstants.ToolName)]
            ),
            new(ChatRole.User, "Store policies and support"),
        ];

        // Act
        var normalized = AGUIWorkflowAgent.NormalizeDisambiguationResume(messages);

        // Assert
        normalized.Count.ShouldBe(3);
        normalized[^1].Role.ShouldBe(ChatRole.Tool);
        var toolResult = normalized[^1].Contents.OfType<FunctionResultContent>().SingleOrDefault();
        toolResult.ShouldNotBeNull();
        toolResult.CallId.ShouldBe("call-123");
        toolResult.Result?.ToString().ShouldBe(Constants.Other.Agents.QAAgent);
    }
}

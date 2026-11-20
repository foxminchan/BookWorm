using System.Text.Json;
using BookWorm.Chat.Orchestration;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration;

public sealed class AGUIClientContextMiddlewareTests
{
    [Test]
    public async Task GivenNoAGUIContext_WhenInvoking_ThenShouldPassMessagesAndOptionsUnchanged()
    {
        // Arrange
        IEnumerable<ChatMessage>? capturedMessages = null;
        ChatOptions? capturedOptions = null;

        var innerClient = new Mock<IChatClient>();
        innerClient
            .Setup(c =>
                c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                (msgs, opts, _) =>
                {
                    capturedMessages = msgs;
                    capturedOptions = opts;
                }
            )
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));

        using var client = innerClient.Object.AsBuilder().UseAGUIClientContext().Build();
        List<ChatMessage> input = [new(ChatRole.User, "Hello")];

        // Act
        await client.GetResponseAsync(input, new());

        // Assert
        if (capturedMessages is not null)
        {
            var chatMessages = capturedMessages as ChatMessage[] ?? [.. capturedMessages];
            chatMessages.ShouldNotBeNull();
            chatMessages.Length.ShouldBe(1);
        }

        capturedOptions.ShouldNotBeNull();
    }

    [Test]
    public async Task GivenAGUIStateContextAndClientTools_WhenInvoking_ThenShouldInjectContextAndMergeTools()
    {
        // Arrange
        List<ChatMessage>? capturedMessages = null;
        ChatOptions? capturedOptions = null;

        var innerClient = new Mock<IChatClient>();
        innerClient
            .Setup(c =>
                c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                (msgs, opts, _) =>
                {
                    capturedMessages = [.. msgs];
                    capturedOptions = opts;
                }
            )
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));

        using var client = innerClient.Object.AsBuilder().UseAGUIClientContext().Build();

        var frontendTool = AIFunctionFactory.Create(() => "shown", "showBookCard");
        var serverTool = AIFunctionFactory.Create(() => "found", "search_catalog");

        var stateElement = JsonDocument.Parse("""{"searchQuery":"dune"}""").RootElement;
        var contextElement = JsonDocument.Parse("""{"itemCount":2}""").RootElement;

        var runOptions = new ChatClientAgentRunOptions(
            new()
            {
                Tools = [frontendTool],
                AdditionalProperties = new()
                {
                    ["ag_ui_state"] = stateElement,
                    ["ag_ui_context"] = contextElement,
                },
            }
        );

        // Act
        using (AGUIRunContext.Push(runOptions))
        {
            await client.GetResponseAsync(
                [new(ChatRole.User, "Find Dune")],
                new() { Tools = [serverTool] }
            );
        }

        // Assert
        AGUIRunContext.Current.ShouldBeNull();
        capturedMessages.ShouldNotBeNull();
        capturedMessages.Count.ShouldBe(2);
        capturedMessages[0].Role.ShouldBe(ChatRole.System);
        capturedMessages[0].Text.ShouldContain("<ag_ui_context>");
        capturedMessages[0].Text.ShouldContain("<ag_ui_state>");
        capturedMessages[0].Text.ShouldContain("dune");

        capturedOptions.ShouldNotBeNull();
        capturedOptions.Tools.ShouldNotBeNull();
        capturedOptions.Tools.Select(t => t.Name).ShouldContain("search_catalog");
        capturedOptions.Tools.Select(t => t.Name).ShouldContain("showBookCard");
    }
}

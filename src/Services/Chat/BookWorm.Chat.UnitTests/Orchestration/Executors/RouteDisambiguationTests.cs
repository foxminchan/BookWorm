using System.Text.Json;
using BookWorm.Chat.Infrastructure.Decisions;
using BookWorm.Chat.Orchestration.Executors.Disambiguation;
using Microsoft.Extensions.AI;

namespace BookWorm.Chat.UnitTests.Orchestration.Executors;

public sealed class RouteDisambiguationTests
{
    [Test]
    [MethodDataSource(nameof(ToAgentIdCases))]
    public void GivenRouteTarget_WhenToAgentId_ThenReturnsExpectedAgentId(
        string targetName,
        string expectedAgentId
    )
    {
        var target = Enum.Parse<RouteTarget>(targetName);
        RouteDisambiguationConstants.ToAgentId(target).ShouldBe(expectedAgentId);
    }

    [Test]
    [MethodDataSource(nameof(TryParseTargetCases))]
    public void GivenInputValue_WhenTryParseTarget_ThenReturnsExpectedResult(
        string? input,
        bool expectedSuccess,
        string expectedTargetName
    )
    {
        var expectedTarget = Enum.Parse<RouteTarget>(expectedTargetName);
        var success = RouteDisambiguationConstants.TryParseTarget(input, out var target);

        success.ShouldBe(expectedSuccess);
        target.ShouldBe(expectedTarget);
    }

    public static IEnumerable<(string, string)> ToAgentIdCases()
    {
        yield return (nameof(RouteTarget.Book), Constants.Other.Agents.BookAgent);
        yield return (nameof(RouteTarget.QA), Constants.Other.Agents.QAAgent);
        yield return (nameof(RouteTarget.Rating), Constants.Other.Agents.RatingAgent);
    }

    public static IEnumerable<(string?, bool, string)> TryParseTargetCases()
    {
        yield return (null, false, nameof(RouteTarget.Book));
        yield return ("", false, nameof(RouteTarget.Book));
        yield return ("   ", false, nameof(RouteTarget.Book));
        yield return ("unknown-route", false, nameof(RouteTarget.Book));
        yield return ("Book", true, nameof(RouteTarget.Book));
        yield return ("find a book", true, nameof(RouteTarget.Book));
        yield return ("1", true, nameof(RouteTarget.Book));
        yield return ("QA", true, nameof(RouteTarget.QA));
        yield return ("customer support", true, nameof(RouteTarget.QA));
        yield return ("return policy", true, nameof(RouteTarget.QA));
        yield return ("store policies", true, nameof(RouteTarget.QA));
        yield return ("2", true, nameof(RouteTarget.QA));
        yield return ("Rating", true, nameof(RouteTarget.Rating));
        yield return ("show ratings", true, nameof(RouteTarget.Rating));
        yield return ("customer review", true, nameof(RouteTarget.Rating));
        yield return ("3", true, nameof(RouteTarget.Rating));
    }

    [Test]
    public void GivenRequest_WhenGetInnerRequestContent_ThenReturnsFunctionCallWithExpectedArguments()
    {
        var request = new RouteDisambiguationRequest(
            "call-1",
            "Which specialist?",
            RouteTarget.QA,
            0.55,
            RouteDisambiguationConstants.DefaultOptions,
            [new(ChatRole.User, "Help")]
        );

        var content = request.GetInnerRequestContent().ShouldBeOfType<FunctionCallContent>();

        content.CallId.ShouldBe("call-1");
        content.Name.ShouldBe(RouteDisambiguationConstants.ToolName);
        content.Arguments.ShouldNotBeNull();
        content.Arguments["question"].ShouldBe("Which specialist?");
        content.Arguments["suggestedRoute"].ShouldBe(Constants.Other.Agents.QAAgent);
        content.Arguments["confidence"].ShouldBe(0.55);
        content.Arguments["options"].ShouldBe(RouteDisambiguationConstants.DefaultOptions);
    }

    [Test]
    public void GivenNullMessages_WhenCreateResponse_ThenThrowsArgumentNullException()
    {
        var request = new RouteDisambiguationRequest(
            "call-1",
            "Which specialist?",
            RouteTarget.Book,
            0.5,
            RouteDisambiguationConstants.DefaultOptions,
            [new(ChatRole.User, "Help")]
        );

        Should.Throw<ArgumentNullException>(() => request.CreateResponse(null!));
    }

    [Test]
    public void GivenFunctionResultFormats_WhenCreateResponse_ThenParsesSelectedTarget()
    {
        var request = new RouteDisambiguationRequest(
            "call-1",
            "Which specialist?",
            RouteTarget.Book,
            0.5,
            RouteDisambiguationConstants.DefaultOptions,
            [new(ChatRole.User, "Help")]
        );

        var jsonString = JsonElement.Parse("\"rating\"");
        var jsonObject = JsonElement.Parse("{\"route\":\"qa\"}");

        var fromJsonString = (RouteDisambiguationResponse)
            request.CreateResponse([
                new(ChatRole.Tool, [new FunctionResultContent("call-1", jsonString)]),
            ]);
        var fromJsonObject = (RouteDisambiguationResponse)
            request.CreateResponse([
                new(ChatRole.Tool, [new FunctionResultContent("call-1", jsonObject)]),
            ]);
        var fromNonStringObject = (RouteDisambiguationResponse)
            request.CreateResponse([new(ChatRole.Tool, [new FunctionResultContent("call-1", 3)])]);
        var fromNullThenText = (RouteDisambiguationResponse)
            request.CreateResponse([
                new(
                    ChatRole.User,
                    [new FunctionResultContent("call-1", null), new TextContent("2")]
                ),
            ]);
        var fallbackToSuggested = (RouteDisambiguationResponse)
            request.CreateResponse([new(ChatRole.User, "not a valid route")]);

        fromJsonString.SelectedTarget.ShouldBe(RouteTarget.Rating);
        fromJsonObject.SelectedTarget.ShouldBe(RouteTarget.QA);
        fromNonStringObject.SelectedTarget.ShouldBe(RouteTarget.Rating);
        fromNullThenText.SelectedTarget.ShouldBe(RouteTarget.QA);
        fallbackToSuggested.SelectedTarget.ShouldBe(RouteTarget.Book);
    }
}

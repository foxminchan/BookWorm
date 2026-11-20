using System.Net;
using System.Text;
using System.Text.Json;
using BookWorm.Chassis.AI.Decisions;

namespace BookWorm.Chat.UnitTests.Infrastructure;

public sealed class FoundrySystemOneDecisionClientTests
{
    [Test]
    public void GivenFoundryProjectConnectionString_WhenParsing_ThenExtractsEndpointAndKey()
    {
        // Act
        var (endpoint, apiKey) = FoundrySystemOneDecisionClient.ParseConnectionString(
            "Endpoint=https://my-foundry.services.ai.azure.com/api/projects/bookworm;Key=secret-key"
        );

        // Assert
        endpoint
            .ToString()
            .ShouldBe("https://my-foundry.services.ai.azure.com/api/projects/bookworm");
        apiKey.ShouldBe("secret-key");
    }

    [Test]
    public void GivenFoundryProjectEndpoint_WhenBuildingSystemOneEndpoint_ThenTargetsProviderRoute()
    {
        // Act
        var systemOne = FoundrySystemOneDecisionClient.BuildSystemOneEndpoint(
            new Uri("https://my-foundry.services.ai.azure.com/api/projects/bookworm")
        );

        // Assert
        systemOne
            .ToString()
            .ShouldBe("https://my-foundry.services.ai.azure.com/providers/microsoft/v1/systemone");
    }

    [Test]
    public async Task GivenMixedDecisionQuestions_WhenCallingGetDecisionAsync_ThenSerializesRequestAndParsesAnswers()
    {
        // Arrange
        string? capturedBody = null;
        string? capturedApiKey = null;
        Uri? capturedUri = null;

        var handler = new StubHttpMessageHandler(
            async (req, ct) =>
            {
                capturedUri = req.RequestUri;
                capturedApiKey = req.Headers.GetValues("api-key").SingleOrDefault();
                capturedBody = await req.Content!.ReadAsStringAsync(ct);

                const string responseJson = """
                    {
                      "model": "microsoft-decision-1",
                      "answers": {
                        "is_english": { "type": "noul", "noul": 0.99 },
                        "route": {
                          "type": "choice",
                          "choice": "book",
                          "confidence": 0.91,
                          "probabilities": { "qa": 0.05, "rating": 0.04, "book": 0.91 }
                        },
                        "urgency": {
                          "type": "score",
                          "score": 1.45,
                          "confidence": 0.78,
                          "probabilities": { "0": 0.1, "1": 0.35, "2": 0.55 }
                        }
                      },
                      "usage": { "input_tokens": 42, "output_tokens": 18 }
                    }
                    """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
                };
            }
        );

        using var httpClient = new HttpClient(handler);
        using var client = new FoundrySystemOneDecisionClient(
            httpClient,
            new Uri("https://my-foundry.services.ai.azure.com/api/projects/bookworm"),
            "decision",
            apiKey: "test-key"
        );

        var request = new DecisionRequest(
            "Recommend a sci-fi book",
            [
                new BinaryDecisionQuestion("is_english", "Is the text in English?"),
                new ChoiceDecisionQuestion(
                    "route",
                    "Select agent",
                    [new("qa", "Store policies"), new("rating", "Reviews"), new("book", "Books")]
                ),
                new ScoreDecisionQuestion(
                    "urgency",
                    "Rate urgency",
                    [new("0", "Low"), new("1", "Medium"), new("2", "High")]
                ),
            ]
        );

        // Act
        var response = await client.GetDecisionAsync(request);

        // Assert
        capturedUri!
            .ToString()
            .ShouldBe("https://my-foundry.services.ai.azure.com/providers/microsoft/v1/systemone");
        capturedApiKey.ShouldBe("test-key");

        using var sentDoc = JsonDocument.Parse(capturedBody!);
        sentDoc.RootElement.GetProperty("model").GetString().ShouldBe("decision");
        sentDoc.RootElement.GetProperty("state").GetString().ShouldBe("Recommend a sci-fi book");
        sentDoc
            .RootElement.GetProperty("questions")
            .GetProperty("is_english")
            .GetProperty("type")
            .GetString()
            .ShouldBe("noul");
        sentDoc
            .RootElement.GetProperty("questions")
            .GetProperty("route")
            .GetProperty("type")
            .GetString()
            .ShouldBe("choice");
        sentDoc
            .RootElement.GetProperty("questions")
            .GetProperty("urgency")
            .GetProperty("type")
            .GetString()
            .ShouldBe("score");

        response.ModelId.ShouldBe("microsoft-decision-1");
        response.Usage.ShouldBe(new DecisionUsage(42, 18));
        response.GetBinaryAnswer("is_english")!.TrueProbability.ShouldBe(0.99);

        var choice = response.GetChoiceAnswer("route")!;
        choice.SelectedCandidateId.ShouldBe("book");
        choice.Confidence.ShouldBe(0.91);
        choice.Probabilities.Count.ShouldBe(3);

        var score = response.GetScoreAnswer("urgency")!;
        score.Score.ShouldBe(1.45);
        score.Confidence.ShouldBe(0.78);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => handler(request, cancellationToken);
    }
}

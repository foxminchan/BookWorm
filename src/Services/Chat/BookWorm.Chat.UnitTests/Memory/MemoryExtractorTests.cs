using BookWorm.Chassis.AI.Presidio;
using BookWorm.Chat.Memory.Extraction;
using BookWorm.Chat.Memory.Semantic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Chat.UnitTests.Memory;

public sealed class MemoryExtractorTests
{
    private readonly Mock<IChatClient> _chatClientMock = new();
    private readonly Mock<IPresidioService> _presidioMock = new();
    private readonly MemoryExtractor _sut;

    public MemoryExtractorTests()
    {
        _presidioMock
            .Setup(p => p.AnonymizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (string text, CancellationToken _) =>
                    text.Replace("john@example.com", "<Pii>").Replace("John Smith", "<Pii>")
            );

        _sut = new(
            _chatClientMock.Object,
            _presidioMock.Object,
            NullLogger<MemoryExtractor>.Instance
        );
    }

    [Test]
    public async Task GivenExchangeWithPii_WhenExtracting_ThenModelShouldOnlyReceiveAnonymizedText()
    {
        IEnumerable<ChatMessage>? sentMessages = null;
        SetupModelResponse(
            """{"episodeSummary":"The customer wants sci-fi.","bookIds":[],"preferences":[]}""",
            messages => sentMessages = messages
        );

        await _sut.ExtractAsync(
            new(
                "user-a",
                "I'm John Smith (john@example.com) and I love science fiction",
                "Here are sci-fi picks"
            )
        );

        sentMessages.ShouldNotBeNull();
        var prompt = string.Join('\n', sentMessages.Select(m => m.Text));
        prompt.ShouldNotContain("John Smith");
        prompt.ShouldNotContain("john@example.com");
        prompt.ShouldContain("<Pii>");
        prompt.ShouldContain("love science fiction");
    }

    [Test]
    public async Task GivenModelOutput_WhenExtracting_ThenShouldReturnValidatedMemories()
    {
        var bookId = Guid.CreateVersion7();
        SetupModelResponse(
            $$"""
            {
              "episodeSummary": "The customer asked for space opera recommendations.",
              "bookIds": ["{{bookId}}", "not-a-guid"],
              "preferences": [
                { "kind": "Genre", "value": "Space Opera", "sentiment": "Like", "explicitlyStated": true },
                { "kind": "Author", "value": "Some Author", "sentiment": "Dislike", "explicitlyStated": true }
              ]
            }
            """
        );

        var result = await _sut.ExtractAsync(new("user-a", "I love space opera", "Sure"));

        result.EpisodeSummary.ShouldBe("The customer asked for space opera recommendations.");
        result.BookIds.ShouldBe([bookId.ToString()]);
        result.Preferences.ShouldBe([
            new(PreferenceKind.Genre, "Space Opera", PreferenceSentiment.Like),
            new(PreferenceKind.Author, "Some Author", PreferenceSentiment.Dislike),
        ]);
    }

    [Test]
    public async Task GivenEmptyUserMessage_WhenExtracting_ThenShouldNotCallModel()
    {
        var result = await _sut.ExtractAsync(new("user-a", " ", "Hello"));

        result.ShouldBe(MemoryExtractionResult.Empty);
        _chatClientMock.Verify(
            c =>
                c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public void GivenInferredSensitiveOrInvalidPreferences_WhenSanitizing_ThenShouldDropThem()
    {
        var payload = new ExtractionPayload
        {
            EpisodeSummary = null,
            Preferences =
            [
                new()
                {
                    Kind = "Genre",
                    Value = "Thrillers",
                    Sentiment = "Like",
                    ExplicitlyStated = false,
                },
                new()
                {
                    Kind = "Theme",
                    Value = "Religious fiction",
                    Sentiment = "Like",
                    ExplicitlyStated = true,
                },
                new()
                {
                    Kind = "Theme",
                    Value = "Mental health",
                    Sentiment = "Like",
                    ExplicitlyStated = true,
                },
                new()
                {
                    Kind = "Author",
                    Value = "<Pii>",
                    Sentiment = "Like",
                    ExplicitlyStated = true,
                },
                new()
                {
                    Kind = "Income",
                    Value = "High",
                    Sentiment = "Like",
                    ExplicitlyStated = true,
                },
                new()
                {
                    Kind = "Genre",
                    Value = "Fantasy",
                    Sentiment = "Maybe",
                    ExplicitlyStated = true,
                },
                new()
                {
                    Kind = "Author",
                    Value = "Grace Paley",
                    Sentiment = "Like",
                    ExplicitlyStated = true,
                },
            ],
        };

        var result = MemoryExtractor.Sanitize(payload);

        result.EpisodeSummary.ShouldBeNull();
        result.Preferences.ShouldBe([
            new(PreferenceKind.Author, "Grace Paley", PreferenceSentiment.Like),
        ]);
    }

    [Test]
    public void GivenNothingMemorable_WhenSanitizing_ThenShouldReturnEmpty()
    {
        var result = MemoryExtractor.Sanitize(new() { EpisodeSummary = "  " });

        result.ShouldBe(MemoryExtractionResult.Empty);
    }

    private void SetupModelResponse(string json, Action<IEnumerable<ChatMessage>>? onRequest = null)
    {
        _chatClientMock
            .Setup(c =>
                c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<IEnumerable<ChatMessage>, ChatOptions?, CancellationToken>(
                (messages, _, _) => onRequest?.Invoke([.. messages])
            )
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, json)));
    }
}

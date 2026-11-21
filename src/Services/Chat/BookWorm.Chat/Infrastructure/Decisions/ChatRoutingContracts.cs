using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BookWorm.Chat.Infrastructure.Decisions;

internal enum RouteTarget : byte
{
    [Description("Route to the Book specialist agent.")]
    Book = 0,

    [Description("Route to the QA customer service specialist agent.")]
    QA = 1,

    [Description("Route to the Rating and review specialist agent.")]
    Rating = 2,

    [Description("Reject non-English user input.")]
    NonEnglish = 3,

    [Description("Reject out-of-scope user input.")]
    OutOfScope = 4,

    [Description("Reject unsafe or prompt-injection user input.")]
    UnsafeInput = 5,
}

internal enum ChatRouteCandidate : byte
{
    [JsonStringEnumMemberName(Constants.Other.Agents.BookAgent)]
    [Description(
        "Book searches, recommendations, book information, conversational book feedback, or review/rating requests where the book is identified only by title, author, or description."
    )]
    Book = 0,

    [JsonStringEnumMemberName(Constants.Other.Agents.QAAgent)]
    [Description(
        "Questions about BookWorm store policies, services, accounts, orders, billing, or service complaints."
    )]
    QA = 1,

    [JsonStringEnumMemberName(Constants.Other.Agents.RatingAgent)]
    [Description(
        "Requests to summarize reviews or analyze ratings ONLY when a specific book catalog ID (such as a UUID/GUID) is already present in the message."
    )]
    Rating = 2,
}

internal sealed record ChatRoutingResult
{
    [JsonPropertyName("is_safe")]
    public double IsSafe { get; init; }

    [JsonPropertyName("is_english")]
    public double IsEnglish { get; init; }

    [JsonPropertyName("is_in_scope")]
    public double IsInScope { get; init; }

    [JsonPropertyName("route")]
    public ChatRouteCandidate Route { get; init; }
}

internal sealed record RoutingDecision(
    RouteTarget Target,
    ChatMessage Message,
    string? RejectionReason = null,
    double Confidence = 1.0,
    IReadOnlyList<ChatMessage>? Messages = null
)
{
    public IReadOnlyList<ChatMessage> EffectiveMessages =>
        Messages is { Count: > 0 } ? Messages : [Message];

    public bool NeedsDisambiguation =>
        RejectionReason is null
        && Confidence < ChatRoutingDecisionService.RouteConfidenceThreshold
        && Target is RouteTarget.Book or RouteTarget.QA or RouteTarget.Rating;
}

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(ChatRoutingResult))]
[JsonSerializable(typeof(ChatRouteCandidate))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
internal sealed partial class ChatRoutingSerializationContext : JsonSerializerContext;

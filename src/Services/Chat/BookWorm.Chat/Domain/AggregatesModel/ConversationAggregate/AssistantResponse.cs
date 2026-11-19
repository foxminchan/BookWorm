using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class AssistantResponse : ValueObject
{
    public const string DefaultFallbackMessage =
        "I apologize, but I couldn't generate a proper response. Could you please try rephrasing your question?";

    private AssistantResponse(string value, bool isFallback)
    {
        Value = value;
        IsFallback = isFallback;
    }

    public string Value { get; }
    public bool IsFallback { get; }

    /// <summary>
    ///     Creates a new <see cref="AssistantResponse" /> instance with the specified formatted content.
    /// </summary>
    /// <param name="formattedContent">The formatted content of the assistant response.</param>
    /// <returns>A new instance of <see cref="AssistantResponse" />.</returns>
    /// <exception cref="ChatDomainException">
    ///     Thrown when the provided <paramref name="formattedContent" /> is null, empty, or consists only of whitespace.
    /// </exception>
    public static AssistantResponse Create(string formattedContent)
    {
        return string.IsNullOrWhiteSpace(formattedContent)
            ? throw new ChatDomainException("Assistant response cannot be empty or whitespace.")
            : new(formattedContent.Trim(), false);
    }

    /// <summary>
    ///     Creates a new <see cref="AssistantResponse" /> instance representing a fallback response.
    /// </summary>
    /// <returns>A new instance of <see cref="AssistantResponse" /> representing a fallback response.</returns>
    public static AssistantResponse Fallback()
    {
        return new(DefaultFallbackMessage, true);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return IsFallback;
    }
}

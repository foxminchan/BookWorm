using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class UserPrompt : ValueObject
{
    public const int MinLength = 1;
    public const int MaxLength = 2000;
    public const int TruncateLength = 1900;
    public const string TruncationSuffix = "... [Message truncated due to length]";

    private UserPrompt(string value, bool wasTruncated)
    {
        Value = value;
        WasTruncated = wasTruncated;
    }

    public string Value { get; }
    public bool WasTruncated { get; }

    /// <summary>
    ///     Creates a new <see cref="UserPrompt" /> instance with the specified raw content.
    /// </summary>
    /// <param name="rawContent">The raw content of the user prompt.</param>
    /// <returns>A new instance of <see cref="UserPrompt" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided <paramref name="rawContent" /> is null.</exception>
    /// <exception cref="ChatDomainException">
    ///     Thrown when the provided <paramref name="rawContent" /> is empty or consists only of whitespace, or exceeds the
    ///     maximum allowed length.
    /// </exception>
    public static UserPrompt Create(string rawContent)
    {
        ArgumentNullException.ThrowIfNull(rawContent);

        var trimmed = rawContent.Trim();

        return trimmed.Length switch
        {
            < MinLength => throw new ChatDomainException(
                "User prompt cannot be empty or whitespace."
            ),
            > MaxLength => new($"{trimmed[..TruncateLength]}{TruncationSuffix}", true),
            _ => new(trimmed, false),
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return WasTruncated;
    }
}

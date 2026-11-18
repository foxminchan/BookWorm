using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class UserScope : ValueObject
{
    private const string AnonymousPrefix = "anonymous-";

    private UserScope(string value, bool isAnonymous)
    {
        Value = value;
        IsAnonymous = isAnonymous;
    }

    public string Value { get; }
    public bool IsAnonymous { get; }

    /// <summary>
    ///     Creates a new <see cref="UserScope" /> instance for an authenticated user with the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID of the authenticated user.</param>
    /// <returns>A new instance of <see cref="UserScope" /> for the authenticated user.</returns>
    /// <exception cref="ChatDomainException">
    ///     Thrown when the provided <paramref name="userId" /> is null, empty, or consists only of whitespace.
    /// </exception>
    public static UserScope ForAuthenticatedUser(string userId)
    {
        return string.IsNullOrWhiteSpace(userId)
            ? throw new ChatDomainException("Authenticated user ID cannot be empty.")
            : new(userId.Trim(), false);
    }

    /// <summary>
    ///     Creates a new <see cref="UserScope" /> instance for an anonymous user with a unique identifier.
    /// </summary>
    /// <returns>A new instance of <see cref="UserScope" /> for the anonymous user.</returns>
    public static UserScope CreateAnonymous()
    {
        return new($"{AnonymousPrefix}{Guid.CreateVersion7()}", true);
    }

    /// <summary>
    ///     Creates a new <see cref="UserScope" /> instance based on the provided user ID.
    ///     If the user ID is null, empty, or consists only of whitespace, an anonymous user scope is created;
    ///     otherwise, an authenticated user scope is created.
    /// </summary>
    /// <param name="userId">The user ID of the authenticated user, or null/empty/whitespace for an anonymous user.</param>
    /// <returns>A new instance of <see cref="UserScope" /> based on the provided user ID.</returns>
    public static UserScope FromUserIdOrAnonymous(string? userId)
    {
        return string.IsNullOrWhiteSpace(userId) ? CreateAnonymous() : ForAuthenticatedUser(userId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return IsAnonymous;
    }
}

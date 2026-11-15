using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;

public sealed class PromptSafetyResult : ValueObject
{
    private PromptSafetyResult(bool isSafe, string? violationReason, string? violationCategory)
    {
        IsSafe = isSafe;
        ViolationReason = violationReason;
        ViolationCategory = violationCategory;
    }

    public bool IsSafe { get; }
    public string? ViolationReason { get; }
    public string? ViolationCategory { get; }

    /// <summary>
    ///     Creates a new <see cref="PromptSafetyResult" /> instance representing a safe prompt.
    /// </summary>
    /// <returns>A new instance of <see cref="PromptSafetyResult" /> indicating the prompt is safe.</returns>
    public static PromptSafetyResult Safe()
    {
        return new(true, null, null);
    }

    /// <summary>
    ///     Creates a new <see cref="PromptSafetyResult" /> instance representing an unsafe prompt
    ///     with the specified violation reason and optional category.
    /// </summary>
    /// <param name="reason">The reason for the prompt being unsafe.</param>
    /// <param name="category">An optional category for the violation.</param>
    /// <returns>A new instance of <see cref="PromptSafetyResult" /> indicating the prompt is unsafe.</returns>
    /// <exception cref="ChatDomainException">
    ///     Thrown when the provided <paramref name="reason" /> is null, empty, or consists only of whitespace.
    /// </exception>
    public static PromptSafetyResult Unsafe(string reason, string? category = null)
    {
        return string.IsNullOrWhiteSpace(reason)
            ? throw new ChatDomainException("Violation reason is required when prompt is unsafe.")
            : new(false, reason, category);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IsSafe;
        yield return ViolationReason;
        yield return ViolationCategory;
    }
}

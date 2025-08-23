using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel;

public sealed class Conversation() : AuditableEntity, IAggregateRoot
{
    private readonly List<ConversationMessage> _messages = [];

    public Conversation(string name, Guid? userId)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ConversationDomainException("Name cannot be null or empty.");
        UserId = userId;
    }

    public string? Name { get; private set; }
    public Guid? UserId { get; private set; }
    public IReadOnlyCollection<ConversationMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    ///     Adds a new message to the conversation.
    /// </summary>
    /// <param name="message">The message to be added to the conversation.</param>
    /// <returns>The current conversation instance to support method chaining.</returns>
    public Conversation AddMessage(ConversationMessage message)
    {
        _messages.Add(message);
        return this;
    }
}

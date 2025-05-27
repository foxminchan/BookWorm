using BookWorm.Chat.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Chat.Domain.AggregatesModel;

public sealed class ConversationMessage() : AuditableEntity
{
    public ConversationMessage(Guid? id, string text, string role, Guid? parentMessageId = null)
        : this()
    {
        Id = id ?? Guid.CreateVersion7();
        Text = !string.IsNullOrWhiteSpace(text)
            ? text
            : throw new ConversationDomainException("Text cannot be null or empty.");
        Role = !string.IsNullOrWhiteSpace(role)
            ? role
            : throw new ConversationDomainException("Role cannot be null or empty.");
        ParentMessageId = parentMessageId;
    }

    public string? Text { get; private set; }
    public string? Role { get; private set; }
    public Guid? ParentMessageId { get; private set; }
}

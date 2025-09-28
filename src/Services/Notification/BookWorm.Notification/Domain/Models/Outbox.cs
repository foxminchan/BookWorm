using BookWorm.SharedKernel.Helpers;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Notification.Domain.Models;

public sealed class Outbox() : IAggregateRoot
{
    public Outbox(string toName, string toEmail, string subject, string body)
        : this()
    {
        ToName = toName;
        ToEmail = toEmail;
        Subject = subject;
        Body = body;
        IsSent = false;
        CreatedAt = DateTimeHelper.UtcNow();
    }

    public Guid Id { get; private set; }
    public string? ToName { get; private set; }
    public string? ToEmail { get; private set; }
    public string? Subject { get; private set; }
    public string? Body { get; private set; }
    public bool IsSent { get; private set; }
    public long SequenceNumber { get; init; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    /// <summary>
    ///     Marks the outbox item as sent by setting the IsSent flag to true and recording the current UTC timestamp.
    /// </summary>
    /// <returns>The current Outbox instance with updated sent status.</returns>
    public Outbox MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTimeHelper.UtcNow();
        return this;
    }
}

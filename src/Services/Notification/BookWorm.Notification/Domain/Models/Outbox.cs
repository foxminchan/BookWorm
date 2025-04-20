namespace BookWorm.Notification.Domain.Models;

public sealed class Outbox(string toName, string toEmail, string subject, string body)
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string ToName { get; private set; } = toName;
    public string ToEmail { get; private set; } = toEmail;
    public string Subject { get; private set; } = subject;
    public string Body { get; private set; } = body;
    public bool IsSent { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; private set; }

    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
    }
}

namespace BookWorm.Notification.Models;

public sealed class EmailOutbox
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Body { get; set; }
    public string? Subject { get; set; }
    public string? To { get; set; }
    public bool IsSent { get; set; }
}

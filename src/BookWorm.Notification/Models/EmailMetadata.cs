namespace BookWorm.Notification.Models;

public sealed class EmailMetadata(string? to, string? subject, string? body)
{
    public string? Body { get; set; } = body;
    public string? Subject { get; set; } = subject;
    public string? To { get; set; } = to;
}

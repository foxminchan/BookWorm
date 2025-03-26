namespace BookWorm.Notification.Infrastructure;

public static class TelemetryTags
{
    public const string ActivitySourceName = "Smtp";
    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);

    public static class SmtpClient
    {
        public const string Recipient = "smtp.recipient";
        public const string Subject = "smtp.subject";
        public const string MessageId = "smtp.messageId";
        public const string EmailOperation = "smtp.operation";
    }
}

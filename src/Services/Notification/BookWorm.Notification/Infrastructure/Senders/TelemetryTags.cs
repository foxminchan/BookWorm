namespace BookWorm.Notification.Infrastructure.Senders;

public static class TelemetryTags
{
    public static class SmtpClient
    {
        public const string Recipient = "smtp.recipient";
        public const string Subject = "smtp.subject";
        public const string MessageId = "smtp.messageId";
        public const string EmailOperation = "smtp.operation";
    }
}

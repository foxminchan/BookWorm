namespace BookWorm.Notification.Extensions;

internal static partial class NotificationTrace
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information,
        Message = "[{Service}] Sending email to {To} with subject {Subject}")]
    public static partial void LogEmailSending(ILogger logger, string? service, string? to, string? subject);

    [LoggerMessage(EventId = 1, Level = LogLevel.Error,
        Message = "[{Service}] Failed to process email to {To} with error: {Error}")]
    public static partial void LogEmailFailed(ILogger logger, string? service, string? to, string? error);
}

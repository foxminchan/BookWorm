namespace BookWorm.Notification.Domain.Exceptions;

public sealed class NotificationException : Exception
{
    public NotificationException(string message)
        : base(message) { }

    public NotificationException(string message, Exception innerException)
        : base(message, innerException) { }
}

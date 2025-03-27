namespace BookWorm.Notification.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class NotificationException(string message) : Exception(message);

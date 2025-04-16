namespace BookWorm.Notification.Domain.Exceptions;

[ExcludeFromCodeCoverage]
public sealed class NotificationException(string message) : Exception(message);

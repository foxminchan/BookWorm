namespace BookWorm.Notification.Domain.Exceptions;

public sealed class NotificationException(string message) : Exception(message);

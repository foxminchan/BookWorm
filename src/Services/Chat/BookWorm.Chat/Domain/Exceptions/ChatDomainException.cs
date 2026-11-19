namespace BookWorm.Chat.Domain.Exceptions;

public sealed class ChatDomainException(string message) : Exception(message);

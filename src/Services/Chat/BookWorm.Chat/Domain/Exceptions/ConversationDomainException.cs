namespace BookWorm.Chat.Domain.Exceptions;

public sealed class ConversationDomainException(string message) : Exception(message);

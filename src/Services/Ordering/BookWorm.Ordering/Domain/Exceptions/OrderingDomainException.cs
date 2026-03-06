namespace BookWorm.Ordering.Domain.Exceptions;

public sealed class OrderingDomainException(string message) : Exception(message);

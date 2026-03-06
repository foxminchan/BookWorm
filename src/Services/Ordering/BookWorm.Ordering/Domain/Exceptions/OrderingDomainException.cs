namespace BookWorm.Ordering.Domain.Exceptions;

internal sealed class OrderingDomainException(string message) : Exception(message);

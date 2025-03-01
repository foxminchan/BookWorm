namespace BookWorm.Ordering.Domain.Exceptions;

public sealed class OutOfStockException(string message) : Exception(message);

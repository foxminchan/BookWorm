namespace BookWorm.Basket.Exceptions;

public sealed class BasketDomainException(string message) : Exception(message);

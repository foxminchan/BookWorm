namespace BookWorm.Basket.Infrastructure.Exceptions;

public sealed class BasketDomainException(string message) : Exception(message);

namespace BookWorm.Basket.Domain;

public sealed class BasketDomainException(string message) : Exception(message);

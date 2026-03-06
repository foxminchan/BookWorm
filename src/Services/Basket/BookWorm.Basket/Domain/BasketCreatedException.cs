namespace BookWorm.Basket.Domain;

public sealed class BasketCreatedException(string message) : Exception(message);

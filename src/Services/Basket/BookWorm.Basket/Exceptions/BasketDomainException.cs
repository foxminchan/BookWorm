namespace BookWorm.Basket.Exceptions;

public sealed class BasketDomainException : Exception
{
    public BasketDomainException() { }

    public BasketDomainException(string message)
        : base(message) { }

    public BasketDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

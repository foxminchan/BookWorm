namespace BookWorm.Catalog.Domain.Exceptions;

public sealed class CatalogDomainException : Exception
{
    public CatalogDomainException() { }

    public CatalogDomainException(string message)
        : base(message) { }

    public CatalogDomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

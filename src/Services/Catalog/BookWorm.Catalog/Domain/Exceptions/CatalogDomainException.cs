namespace BookWorm.Catalog.Domain.Exceptions;

public sealed class CatalogDomainException(string message) : Exception(message);

namespace BookWorm.Catalog.Infrastructure.Blob;

internal sealed class UrnException(string message, string paramName = "urn")
    : ArgumentException(message, paramName);

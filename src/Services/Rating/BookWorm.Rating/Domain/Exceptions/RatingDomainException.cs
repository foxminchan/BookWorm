namespace BookWorm.Rating.Domain.Exceptions;

internal sealed class RatingDomainException(string message) : Exception(message);

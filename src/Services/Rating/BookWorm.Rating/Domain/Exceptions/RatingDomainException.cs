namespace BookWorm.Rating.Domain.Exceptions;

public sealed class RatingDomainException(string message) : Exception(message);

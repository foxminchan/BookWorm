namespace BookWorm.Finance.Domain.Exceptions;

public sealed class FinanceDomainException(string message) : Exception(message);

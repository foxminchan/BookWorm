using BookWorm.Finance.Domain.Exceptions;
using BookWorm.SharedKernel.SeedWork;

namespace BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;

public sealed class Money : ValueObject
{
    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new FinanceDomainException("Total money cannot be negative.");
        }

        Amount = amount;
    }

    public decimal Amount { get; }

    public static Money Of(decimal amount)
    {
        return new(amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}

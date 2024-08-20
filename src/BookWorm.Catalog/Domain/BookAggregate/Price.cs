namespace BookWorm.Catalog.Domain.BookAggregate;

public sealed class Price : ValueObject
{
    public Price() { }

    public Price(decimal originalPrice, decimal? discountPrice)
    {
        OriginalPrice = Guard.Against.OutOfRange(originalPrice, nameof(originalPrice), 0, decimal.MaxValue);
        DiscountPrice = Guard.Against.OutOfRange(discountPrice ?? -1, nameof(discountPrice), -1, decimal.MaxValue);
    }

    public decimal OriginalPrice { get; }
    public decimal? DiscountPrice { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OriginalPrice;
        yield return DiscountPrice ?? -1;
    }
}

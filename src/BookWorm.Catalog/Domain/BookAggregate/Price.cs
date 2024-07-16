using BookWorm.Core.SeedWork;

namespace BookWorm.Catalog.Domain.BookAggregate;

public sealed class Price : ValueObject
{
    public Price() { }

    public Price(decimal originalPrice, decimal? discountPrice)
    {
        OriginalPrice = originalPrice;
        DiscountPrice = discountPrice;
    }

    public decimal OriginalPrice { get; }
    public decimal? DiscountPrice { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OriginalPrice;
        yield return DiscountPrice ?? -1;
    }
}

using BookWorm.Core.SeedWork;

namespace BookWorm.Catalog.Domain.BookAggregate;

public sealed class Price : ValueObject
{
    public decimal OriginalPrice { get; private set; }
    public decimal? DiscountPrice { get; private set; }

    public Price() { }

    public Price(decimal originalPrice, decimal? discountPrice)
    {
        OriginalPrice = originalPrice;
        DiscountPrice = discountPrice;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OriginalPrice;
        yield return DiscountPrice ?? -1;
    }
}

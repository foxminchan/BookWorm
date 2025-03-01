namespace BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;

public sealed class Price() : ValueObject
{
    public Price(decimal originalPrice, decimal? discountPrice)
        : this()
    {
        OriginalPrice =
            originalPrice < 0
                ? throw new CatalogDomainException(
                    "Original price must be greater than or equal to 0."
                )
                : originalPrice;
        DiscountPrice =
            discountPrice < 0 || discountPrice > originalPrice
                ? throw new CatalogDomainException(
                    "Discount price must be greater than or equal to 0 and less than or equal to original price."
                )
                : discountPrice;
    }

    public decimal OriginalPrice { get; }
    public decimal? DiscountPrice { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OriginalPrice;
        yield return DiscountPrice ?? 0;
    }
}

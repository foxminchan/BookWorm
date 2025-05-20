namespace BookWorm.Ordering.Features.Buyers;

public static class DomainToDtoMapper
{
    public static BuyerDto ToBuyerDto(this Buyer buyer)
    {
        return new(buyer.Id, buyer.Name, buyer.FullAddress);
    }

    public static IReadOnlyList<BuyerDto> ToBuyerDtos(this IEnumerable<Buyer> buyers)
    {
        return [.. buyers.Select(b => b.ToBuyerDto())];
    }
}

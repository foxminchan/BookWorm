namespace BookWorm.Ordering.Features.Buyers;

internal static class DomainToDtoMapper
{
    extension(Buyer buyer)
    {
        public BuyerDto ToBuyerDto()
        {
            return new(buyer.Id, buyer.Name, buyer.FullAddress);
        }
    }

    extension(IEnumerable<Buyer> buyers)
    {
        public IReadOnlyList<BuyerDto> ToBuyerDtos()
        {
            return [.. buyers.Select(b => b.ToBuyerDto())];
        }
    }
}

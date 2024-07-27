using BookWorm.Ordering.Domain.BuyerAggregate;

namespace BookWorm.Ordering.Features.Buyers;

public static class EntityToDto
{
    public static BuyerDto ToBuyerDto(this Buyer buyer)
    {
        return new(buyer.Id, buyer.Name, $"{buyer.Address?.Street}, {buyer.Address?.City}, {buyer.Address?.Province}");
    }

    public static List<BuyerDto> ToBuyerDtos(this IEnumerable<Buyer> buyers)
    {
        return buyers.Select(ToBuyerDto).ToList();
    }
}

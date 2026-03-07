namespace BookWorm.Basket.Features;

internal static class ContractToDomainMapper
{
    public static List<BasketItem> ToBasketItem(this List<BasketItemRequest> items)
    {
        return [.. items.Select(item => new BasketItem(item.Id, item.Quantity))];
    }
}

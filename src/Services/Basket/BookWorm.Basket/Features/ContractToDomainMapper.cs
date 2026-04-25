namespace BookWorm.Basket.Features;

internal static class ContractToDomainMapper
{
    extension(List<BasketItemRequest> items)
    {
        public List<BasketItem> ToBasketItem()
        {
            return [.. items.Select(item => new BasketItem(item.Id, item.Quantity))];
        }
    }
}

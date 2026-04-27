namespace BookWorm.Basket.Features;

internal static class ContractToDomainMapper
{
    extension(List<BasketItemRequest> items)
    {
        public IReadOnlyList<BasketItem> ToBasketItem()
        {
            return [.. items.Select(item => new BasketItem(item.Id, item.Quantity))];
        }
    }
}

namespace BookWorm.Basket.Features;

internal static class DomainToDtoMapper
{
    extension(CustomerBasket model)
    {
        public CustomerBasketDto ToCustomerBasketDto()
        {
            return new(
                (string)model.Id,
                [.. model.Items.Select(x => new BasketItemDto(x.Id, x.Quantity))]
            );
        }
    }
}

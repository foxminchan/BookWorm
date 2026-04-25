namespace BookWorm.Basket.Features;

internal static class DomainToDtoMapper
{
    extension(CustomerBasket model)
    {
        public CustomerBasketDto ToCustomerBasketDto()
        {
            return new(model.Id, [.. model.Items.Select(x => new BasketItemDto(x.Id, x.Quantity))]);
        }
    }
}

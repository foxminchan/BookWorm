﻿namespace BookWorm.Basket.Features;

public static class DomainToDtoMapper
{
    public static CustomerBasketDto ToCustomerBasketDto(this CustomerBasket model)
    {
        return new(model.Id, [.. model.Items.Select(x => new BasketItemDto(x.Id, x.Quantity))]);
    }
}

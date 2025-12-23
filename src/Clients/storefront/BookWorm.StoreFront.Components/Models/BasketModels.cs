namespace BookWorm.StoreFront.Components.Models;

public sealed record BasketItem(string? Id, int Quantity, string? Name, decimal Price, decimal? PriceSale);

public sealed record CustomerBasket(string? Id, List<BasketItem> Items);

public sealed record BasketItemRequest(string Id, int Quantity);

public sealed record CreateBasketRequest(List<BasketItemRequest> Items);

public sealed record UpdateBasketRequest(List<BasketItemRequest> Items);

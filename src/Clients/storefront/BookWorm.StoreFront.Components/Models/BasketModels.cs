namespace BookWorm.StoreFront.Components.Models;

public sealed record BasketItem(Guid BookId, string BookTitle, decimal Price, int Quantity);

public sealed record Basket(Guid Id, List<BasketItem> Items, decimal TotalPrice);

public sealed record AddToBasketRequest(Guid BookId, int Quantity);

public sealed record UpdateBasketItemRequest(Guid BookId, int Quantity);

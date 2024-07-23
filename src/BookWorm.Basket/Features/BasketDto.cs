namespace BookWorm.Basket.Features;

public sealed record BasketDto(
    Guid Id,
    List<BasketItemDto> Items,
    decimal TotalPrice);

public sealed record BasketItemDto(
    Guid BookId,
    string Name,
    int Quantity,
    decimal Price,
    decimal PriceSale);

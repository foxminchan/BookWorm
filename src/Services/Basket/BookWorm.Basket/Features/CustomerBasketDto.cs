namespace BookWorm.Basket.Features;

public sealed record CustomerBasketDto(
    [StringSyntax(StringSyntaxAttribute.GuidFormat)] string? Id,
    List<BasketItemDto> Items
);

public sealed record BasketItemDto(
    [StringSyntax(StringSyntaxAttribute.GuidFormat)] string? Id,
    int Quantity,
    string? Name = null,
    decimal Price = 0,
    decimal? PriceSale = null
);

namespace BookWorm.Basket.Features;

public sealed record CustomerBasketDto(string? Id, List<BasketItemDto> Items);

public sealed record BasketItemDto(string? Id, int Quantity)
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public decimal? PriceSale { get; set; }
}

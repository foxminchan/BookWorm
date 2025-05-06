namespace BookWorm.Basket.Features;

public sealed record BasketItemRequest(
    [StringSyntax(StringSyntaxAttribute.GuidFormat)] string Id,
    int Quantity
);

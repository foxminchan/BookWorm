namespace BookWorm.Ordering.Features.Buyers;

public sealed record BuyerDto(
    BuyerId Id,
    [property: PIIData] string? Name,
    [property: PIIData] string? Address
);

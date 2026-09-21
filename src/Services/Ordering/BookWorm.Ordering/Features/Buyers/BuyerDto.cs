namespace BookWorm.Ordering.Features.Buyers;

public sealed record BuyerDto(BuyerId Id, [PIIData] string? Name, [PIIData] string? Address);

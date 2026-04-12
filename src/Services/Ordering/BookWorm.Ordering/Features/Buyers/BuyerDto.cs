namespace BookWorm.Ordering.Features.Buyers;

public sealed record BuyerDto(Guid Id, [PIIData] string? Name, [PIIData] string? Address);

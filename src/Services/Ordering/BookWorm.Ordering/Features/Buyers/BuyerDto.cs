namespace BookWorm.Ordering.Features.Buyers;

public sealed record BuyerDto(Guid Id, string? Name, [PIIData] string? Address);

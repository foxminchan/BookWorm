namespace BookWorm.Ordering.Features.Buyers;

public sealed record BuyerDto(Guid Id, string? Name, [PiiData] string? Address);

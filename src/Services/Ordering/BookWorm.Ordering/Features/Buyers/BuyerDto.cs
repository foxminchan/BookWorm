namespace BookWorm.Ordering.Features.Buyers;

/// <summary>
///     Buyer projection returned only to the authenticated owner or an admin.
///     <see cref="Address" /> is PII and may only be returned from endpoints that enforce ownership or admin authorization, including list or search endpoints.
///     The endpoint serving this DTO must enforce ownership or admin authorization.
/// </summary>
public sealed record BuyerDto(Guid Id, [PIIData] string? Name, [PIIData] string? Address);

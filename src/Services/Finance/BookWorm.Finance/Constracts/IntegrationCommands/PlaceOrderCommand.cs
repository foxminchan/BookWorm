namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Finance)]
public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? FullName,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;

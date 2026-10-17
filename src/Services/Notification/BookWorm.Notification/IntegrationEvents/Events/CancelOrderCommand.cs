namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Finance)]
public sealed record CancelOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

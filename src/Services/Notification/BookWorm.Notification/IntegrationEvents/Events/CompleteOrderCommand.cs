namespace BookWorm.Contracts;

[IntegrationMessageOwner(BookWorm.Constants.Aspire.Services.Finance)]
public sealed record CompleteOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Finance, TopicName = "delete-basket-failed")]
public sealed record DeleteBasketFailedCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;

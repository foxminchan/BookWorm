using BookWorm.Constants.Aspire;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Finance, TopicName = "delete-basket-failed")]
public sealed record DeleteBasketFailedCommand(
    Guid BasketId,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;

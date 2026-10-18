using BookWorm.Constants.Aspire;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Finance, TopicName = "place-order")]
public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? FullName,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;

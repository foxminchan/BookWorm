namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Finance, TopicName = "place-order")]
public sealed record PlaceOrderCommand(
    Guid BasketId,
    string? FullName,
    string? Email,
    Guid OrderId,
    decimal TotalMoney
) : IntegrationEvent;

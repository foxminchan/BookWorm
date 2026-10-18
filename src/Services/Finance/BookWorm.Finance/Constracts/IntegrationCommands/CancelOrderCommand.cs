namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Finance, TopicName = "cancel-order")]
public sealed record CancelOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

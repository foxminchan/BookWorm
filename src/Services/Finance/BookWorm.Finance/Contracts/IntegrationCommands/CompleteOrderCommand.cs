namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Finance, TopicName = "complete-order")]
public sealed record CompleteOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

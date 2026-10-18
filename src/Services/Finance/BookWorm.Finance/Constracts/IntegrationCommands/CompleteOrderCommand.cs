namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = BookWorm.Constants.Aspire.Services.Finance, TopicName = "complete-order")]
public sealed record CompleteOrderCommand(
    Guid OrderId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

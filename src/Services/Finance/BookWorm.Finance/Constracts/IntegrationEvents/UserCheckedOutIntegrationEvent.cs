using BookWorm.Constants.Aspire;
using Wolverine.Attributes;

namespace BookWorm.Contracts;

[IntegrationMessageOwner(ServiceName = Services.Ordering, TopicName = "user-checked-out")]
[MessageIdentity("BookWorm.Contracts.UserCheckedOutIntegrationEvent")]
public sealed record UserCheckedOutIntegrationEvent(
    Guid OrderId,
    Guid BasketId,
    string? FullName,
    string? Email,
    decimal TotalMoney
) : IntegrationEvent;

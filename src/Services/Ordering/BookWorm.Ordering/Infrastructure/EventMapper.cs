using BookWorm.Contracts;
using Microsoft.Identity.Web;

namespace BookWorm.Ordering.Infrastructure;

internal sealed class EventMapper(ClaimsPrincipal claimsPrincipal) : IEventMapper
{
    public IntegrationEvent MapToIntegrationEvent(DomainEvent @event)
    {
        var email = claimsPrincipal[ClaimTypes.Email];
        var fullName = claimsPrincipal[ClaimConstants.Name];

        return @event switch
        {
            OrderPlacedEvent orderPlacedEvent => new UserCheckedOutIntegrationEvent(
                (Guid)orderPlacedEvent.Order.Id,
                (Guid)orderPlacedEvent.Order.BuyerId,
                fullName,
                email,
                orderPlacedEvent.Order.TotalPrice
            ),
            OrderCancelledEvent orderCancelledEvent =>
                new OrderStatusChangedToCancelIntegrationEvent(
                    (Guid)orderCancelledEvent.Order.Id,
                    (Guid)orderCancelledEvent.Order.BuyerId,
                    fullName,
                    email,
                    orderCancelledEvent.Order.TotalPrice
                ),
            OrderCompletedEvent completedEvent => new OrderStatusChangedToCompleteIntegrationEvent(
                (Guid)completedEvent.Order.Id,
                (Guid)completedEvent.Order.BuyerId,
                fullName,
                email,
                completedEvent.Order.TotalPrice
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null),
        };
    }
}

using BookWorm.Contracts;

namespace BookWorm.Ordering.Infrastructure.Services;

public sealed class EventMapper(ClaimsPrincipal claimsPrincipal) : IEventMapper
{
    public IntegrationEvent MapToIntegrationEvent(DomainEvent @event)
    {
        return @event switch
        {
            OrderPlacedEvent orderPlacedEvent => new UserCheckedOutIntegrationEvent(
                orderPlacedEvent.Order.Id,
                orderPlacedEvent.Order.BuyerId,
                claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Email),
                orderPlacedEvent.Order.TotalPrice
            ),
            OrderCancelledEvent orderCancelledEvent =>
                new OrderStatusChangedToCancelIntegrationEvent(
                    orderCancelledEvent.Order.Id,
                    orderCancelledEvent.Order.BuyerId,
                    claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Email),
                    orderCancelledEvent.Order.TotalPrice
                ),
            OrderCompletedEvent completedEvent => new OrderStatusChangedToCompleteIntegrationEvent(
                completedEvent.Order.Id,
                completedEvent.Order.BuyerId,
                claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Email),
                completedEvent.Order.TotalPrice
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null),
        };
    }
}

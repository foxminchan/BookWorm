using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Contracts;

namespace BookWorm.Ordering.Infrastructure.Services;

internal sealed class EventMapper(ClaimsPrincipal claimsPrincipal) : IEventMapper
{
    public IntegrationEvent MapToIntegrationEvent(DomainEvent @event)
    {
        var email = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Email);
        var fullName = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Name);

        return @event switch
        {
            OrderPlacedEvent orderPlacedEvent => new UserCheckedOutIntegrationEvent(
                orderPlacedEvent.Order.Id,
                orderPlacedEvent.Order.BuyerId,
                fullName,
                email,
                orderPlacedEvent.Order.TotalPrice
            ),
            OrderCancelledEvent orderCancelledEvent =>
                new OrderStatusChangedToCancelIntegrationEvent(
                    orderCancelledEvent.Order.Id,
                    orderCancelledEvent.Order.BuyerId,
                    fullName,
                    email,
                    orderCancelledEvent.Order.TotalPrice
                ),
            OrderCompletedEvent completedEvent => new OrderStatusChangedToCompleteIntegrationEvent(
                completedEvent.Order.Id,
                completedEvent.Order.BuyerId,
                fullName,
                email,
                completedEvent.Order.TotalPrice
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null),
        };
    }
}

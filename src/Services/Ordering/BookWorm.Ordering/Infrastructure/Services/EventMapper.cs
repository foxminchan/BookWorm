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

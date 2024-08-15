using Ardalis.GuardClauses;
using BookWorm.Core.SeedWork;

namespace BookWorm.Notification.IntegrationEvents.Events;

public sealed class OrderCompletedIntegrationEvent(Guid orderId, string? email) : IIntegrationEvent
{
    public Guid OrderId { get; init; } = Guard.Against.Default(orderId);
    public string? Email { get; init; } = email;
}

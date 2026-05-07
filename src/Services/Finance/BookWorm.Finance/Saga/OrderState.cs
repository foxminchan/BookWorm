using BookWorm.Contracts;
using Wolverine.Persistence.Sagas;

namespace BookWorm.Finance.Saga;

/// <summary>
///     Persisted state for the order-placement saga. Replaces the
///     MassTransit <c>SagaStateMachineInstance</c> wrapper — the same
///     domain fields, repackaged for Wolverine's <see cref="Saga" /> base.
/// </summary>
internal sealed class OrderState
{
    /// <summary>Correlation key — maps 1-to-1 with the saga <c>Id</c> in Wolverine.</summary>
    [SagaIdentity]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }

    /// <summary>Persisted as text via enum-to-string conversion for backward-readable diagnostics.</summary>
    public OrderSagaStatus CurrentState { get; set; }

    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; set; }
    public int TimeoutRetryCount { get; set; }
    public uint RowVersion { get; init; }

    public bool CanSendNotification =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(FullName);

    public decimal EffectiveTotalMoney => TotalMoney.GetValueOrDefault(0.0M);

    public void IncrementRetry()
    {
        TimeoutRetryCount++;
    }

    public bool HasExceededMaxRetries(int maxAttempts)
    {
        return TimeoutRetryCount >= maxAttempts;
    }

    public void MapFrom(UserCheckedOutIntegrationEvent @event)
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        TotalMoney = @event.TotalMoney;
        FullName = @event.FullName;
    }

    public void MapFrom(BasketDeletedFailedIntegrationEvent @event)
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        TotalMoney = @event.TotalMoney;
    }

    public void MapFrom(BasketDeletedCompleteIntegrationEvent @event)
    {
        BasketId = @event.BasketId;
        OrderId = @event.OrderId;
        TotalMoney = @event.TotalMoney;
    }

    public void MapFrom(OrderStatusChangedToCompleteIntegrationEvent @event)
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;
    }

    public void MapFrom(OrderStatusChangedToCancelIntegrationEvent @event)
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;
    }
}

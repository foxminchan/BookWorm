using BookWorm.Contracts;

namespace BookWorm.Finance.Saga;

public sealed class OrderState : SagaStateMachineInstance, ISagaVersion
{
    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string CurrentState { get; init; } = null!;
    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; set; }
    public Guid? PlaceOrderTimeoutTokenId { get; init; }
    public int TimeoutRetryCount { get; set; }
    public uint RowVersion { get; init; }

    public bool CanSendNotification =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(FullName);

    public decimal EffectiveTotalMoney => TotalMoney.GetValueOrDefault(0.0M);
    public int Version { get; set; }
    public Guid CorrelationId { get; set; }

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

using BookWorm.Contracts;
using BookWorm.SharedKernel.Helpers;
using Wolverine;

namespace BookWorm.Finance.Saga;

/// <summary>
///     Wolverine saga that orchestrates the order-placement flow, replacing the
///     MassTransit <c>OrderStateMachine</c>. Correlation key is <c>OrderId</c>
///     (carried by every integration event in the flow).
/// </summary>
internal sealed class OrderSaga : global::Wolverine.Saga
{
    // ── Persisted state ────────────────────────────────────────────────────

    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public OrderSagaStatus CurrentState { get; set; }
    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; set; }
    public int TimeoutRetryCount { get; set; }
    public uint RowVersion { get; init; }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private bool CanSendNotification =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(FullName);

    private decimal EffectiveTotalMoney => TotalMoney.GetValueOrDefault(0.0M);

    // ── Start ────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Creates the saga and schedules the initial timeout.
    ///     The returned <see cref="PlaceOrderTimeout" /> is automatically
    ///     enqueued by Wolverine's saga runtime.
    /// </summary>
    public static (OrderSaga, OutgoingMessages) Start(
        UserCheckedOutIntegrationEvent @event,
        OrderStateMachineSettings settings,
        ILogger<OrderSaga> logger
    )
    {
        var saga = new OrderSaga
        {
            Id = @event.OrderId,
            OrderId = @event.OrderId,
            BasketId = @event.BasketId,
            Email = @event.Email,
            TotalMoney = @event.TotalMoney,
            FullName = @event.FullName,
            OrderPlacedDate = DateTimeHelper.UtcNow(),
            CurrentState = OrderSagaStatus.Placed,
        };

        logger.LogInformation(
            "[{Saga}] Order saga started for {OrderId}",
            nameof(OrderSaga),
            @event.OrderId
        );

        var messages = new OutgoingMessages
        {
            new PlaceOrderCommand(
                saga.BasketId,
                saga.FullName,
                saga.Email,
                saga.OrderId,
                saga.EffectiveTotalMoney
            ),
            new PlaceOrderTimeout(@event.OrderId, settings.MaxRetryTimeout),
        };

        return (saga, messages);
    }

    // ── Timeout ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Handles the recurring timeout. Re-issues <see cref="PlaceOrderCommand" />
    ///     until <see cref="OrderStateMachineSettings.MaxAttempts" /> is reached,
    ///     then cancels the order.
    /// </summary>
    public OutgoingMessages Handle(PlaceOrderTimeout timeout, OrderStateMachineSettings settings)
    {
        TimeoutRetryCount++;

        if (!HasExceededMaxRetries(settings.MaxAttempts))
        {
            var messages = new OutgoingMessages
            {
                new PlaceOrderCommand(BasketId, FullName, Email, OrderId, EffectiveTotalMoney),
                new PlaceOrderTimeout(OrderId, settings.MaxRetryTimeout),
            };
            return messages;
        }

        // Max retries exceeded — cancel the order.
        CurrentState = OrderSagaStatus.Cancelled;
        MarkCompleted();

        var finalMessages = new OutgoingMessages();

        if (CanSendNotification)
        {
            finalMessages.Add(
                new CancelOrderCommand(OrderId, FullName!, Email!, EffectiveTotalMoney)
            );
        }

        return finalMessages;
    }

    // ── Basket deletion response ─────────────────────────────────────────────

    /// <summary>Basket deleted successfully — fire notification to Ordering service.</summary>
    public DeleteBasketCompleteCommand Handle(BasketDeletedCompleteIntegrationEvent @event)
    {
        BasketId = @event.BasketId;
        OrderId = @event.OrderId;
        TotalMoney = @event.TotalMoney;

        return new DeleteBasketCompleteCommand(OrderId, EffectiveTotalMoney);
    }

    /// <summary>Basket deletion failed — mark terminal state.</summary>
    public OutgoingMessages Handle(
        BasketDeletedFailedIntegrationEvent @event,
        ILogger<OrderSaga> logger
    )
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        TotalMoney = @event.TotalMoney;

        CurrentState = OrderSagaStatus.BasketDeletionFailed;

        logger.LogError(
            "[{Saga}] Basket deletion failed for {OrderId}",
            nameof(OrderSaga),
            @event.OrderId
        );

        MarkCompleted();

        return [new DeleteBasketFailedCommand(BasketId, Email, OrderId, EffectiveTotalMoney)];
    }

    // ── Order status changes ─────────────────────────────────────────────────

    /// <summary>Order completed by Ordering service — notify customer and finalize.</summary>
    public OutgoingMessages Handle(
        OrderStatusChangedToCompleteIntegrationEvent @event,
        ILogger<OrderSaga> logger
    )
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;

        CurrentState = OrderSagaStatus.Completed;

        logger.LogInformation(
            "[{Saga}] Order saga completed for {OrderId}",
            nameof(OrderSaga),
            @event.OrderId
        );

        MarkCompleted();

        var messages = new OutgoingMessages();

        if (CanSendNotification)
        {
            messages.Add(new CompleteOrderCommand(OrderId, FullName!, Email!, EffectiveTotalMoney));
        }

        return messages;
    }

    /// <summary>Order cancelled by Ordering service — notify customer and finalize.</summary>
    public OutgoingMessages Handle(
        OrderStatusChangedToCancelIntegrationEvent @event,
        ILogger<OrderSaga> logger
    )
    {
        OrderId = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;

        CurrentState = OrderSagaStatus.Cancelled;

        logger.LogInformation(
            "[{Saga}] Order saga cancelled for {OrderId}",
            nameof(OrderSaga),
            @event.OrderId
        );

        MarkCompleted();

        var messages = new OutgoingMessages();

        if (CanSendNotification)
        {
            messages.Add(new CancelOrderCommand(OrderId, FullName!, Email!, EffectiveTotalMoney));
        }

        return messages;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private bool HasExceededMaxRetries(int maxAttempts) => TimeoutRetryCount >= maxAttempts;
}

/// <summary>Represents the lifecycle state of an order saga.</summary>
internal enum OrderSagaStatus
{
    Placed,
    BasketDeletionFailed,
    Completed,
    Cancelled,
}

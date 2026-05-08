using BookWorm.Contracts;
using BookWorm.SharedKernel.Helpers;
using Wolverine;

namespace BookWorm.Finance.Saga;

internal sealed class OrderSaga : Wolverine.Saga
{
    public Guid Id { get; init; }

    public Guid OrderId { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public OrderSagaStatus CurrentState { get; set; }
    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; init; }
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
    /// <param name="event">The initial event that triggers the saga, containing order details from the Basket service.</param>
    /// <param name="settings">Configuration settings for the saga, including retry policies and timeouts.</param>
    /// <param name="logger">Logger instance for recording saga progress and issues.</param>
    /// <returns></returns>
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
    /// <param name="timeout">
    ///     A timeout message triggered by Wolverine's saga runtime after the specified delay, containing the
    ///     order ID.
    /// </param>
    /// <param name="settings">Configuration settings for the saga, including retry policies and timeouts.</param>
    /// <returns></returns>
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

    /// <summary>
    ///     Basket deleted successfully — proceed to complete the order.
    /// </summary>
    /// <param name="event">An event containing successful basket deletion details from the Basket service.</param>
    /// <returns></returns>
    public DeleteBasketCompleteCommand Handle(BasketDeletedCompleteIntegrationEvent @event)
    {
        BasketId = @event.BasketId;
        OrderId = @event.OrderId;
        TotalMoney = @event.TotalMoney;

        return new(OrderId, EffectiveTotalMoney);
    }

    /// <summary>
    ///     Basket deletion failed — log the error, mark the saga as failed, and finalize.
    /// </summary>
    /// <param name="event">An event containing failed basket deletion details from the Basket service.</param>
    /// <param name="logger">Logger instance for recording saga progress and issues.</param>
    /// <returns></returns>
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

    /// <summary>
    ///     Order completed by Ordering service — fire notification to customer and finalize.
    /// </summary>
    /// <param name="event">An event containing order completion details from the Ordering service.</param>
    /// <param name="logger">Logger instance for recording saga progress and issues.</param>
    /// <returns></returns>
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

    /// <summary>
    ///     Order canceled by Ordering service — fire cancellation notification to customer and finalize.
    /// </summary>
    /// <param name="event">An event containing order completion details from the Ordering service.</param>
    /// <param name="logger">Logger instance for recording saga progress and issues.</param>
    /// <returns></returns>
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

    private bool HasExceededMaxRetries(int maxAttempts)
    {
        return TimeoutRetryCount >= maxAttempts;
    }
}

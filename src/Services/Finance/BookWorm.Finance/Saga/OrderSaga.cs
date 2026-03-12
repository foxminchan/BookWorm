using System.Diagnostics;
using System.Diagnostics.Metrics;
using BookWorm.Contracts;
using BookWorm.SharedKernel.Helpers;
using Microsoft.Extensions.Options;

namespace BookWorm.Finance.Saga;

public sealed class OrderSaga : Wolverine.Saga
{
    private static readonly ActivitySource _activitySource = new(nameof(OrderSaga));
    private static readonly Meter _sagaMeter = new(nameof(OrderSaga));

    private static readonly Counter<long> _stateTransitionCounter = _sagaMeter.CreateCounter<long>(
        "finance.saga.state_transitions",
        description: "Number of order saga state transitions"
    );

    public Guid Id { get; set; }
    public Guid BasketId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string CurrentState { get; set; } = "Initial";
    public decimal? TotalMoney { get; set; }
    public DateTime? OrderPlacedDate { get; set; }
    public int TimeoutRetryCount { get; set; }
    public bool IsTimeoutCancelled { get; set; }

    private bool CanSendNotification =>
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(FullName);

    private decimal EffectiveTotalMoney => TotalMoney.GetValueOrDefault(0.0M);

    /// <summary>
    /// Starts the saga when a user checks out.
    /// Maps event data, transitions to Placed, schedules a timeout, and publishes a place-order command.
    /// </summary>
    public async Task Start(
        UserCheckedOutIntegrationEvent @event,
        IMessageContext context,
        IOptions<OrderStateMachineSettings> options
    )
    {
        Id = @event.OrderId;
        BasketId = @event.BasketId;
        Email = @event.Email;
        TotalMoney = @event.TotalMoney;
        FullName = @event.FullName;
        OrderPlacedDate = DateTimeHelper.UtcNow();

        TransitionTo("Placed");

        await context.ScheduleAsync(
            new PlaceOrderTimeoutIntegrationEvent(Id),
            options.Value.MaxRetryTimeout
        );

        await context.PublishAsync(
            new PlaceOrderCommand(BasketId, FullName, Email, Id, EffectiveTotalMoney)
        );
    }

    /// <summary>
    /// Handles successful basket deletion. Stays in Placed state and logically cancels the timeout.
    /// </summary>
    public async Task Handle(BasketDeletedCompleteIntegrationEvent @event, IMessageContext context)
    {
        if (CurrentState != "Placed")
        {
            return;
        }

        BasketId = @event.BasketId;
        TotalMoney = @event.TotalMoney;
        IsTimeoutCancelled = true;

        await context.PublishAsync(new DeleteBasketCompleteCommand(Id, EffectiveTotalMoney));
    }

    /// <summary>
    /// Handles basket deletion failure. Transitions to Failed.
    /// </summary>
    public async Task Handle(BasketDeletedFailedIntegrationEvent @event, IMessageContext context)
    {
        if (CurrentState != "Placed")
        {
            return;
        }

        BasketId = @event.BasketId;
        Email = @event.Email;
        TotalMoney = @event.TotalMoney;
        IsTimeoutCancelled = true;

        TransitionTo("Failed");

        await context.PublishAsync(
            new DeleteBasketFailedCommand(BasketId, Email, Id, EffectiveTotalMoney)
        );
    }

    /// <summary>
    /// Handles order completion. Transitions to Completed and finalizes the saga.
    /// </summary>
    public async Task Handle(
        OrderStatusChangedToCompleteIntegrationEvent @event,
        IMessageContext context
    )
    {
        if (CurrentState != "Placed")
        {
            return;
        }

        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;
        IsTimeoutCancelled = true;

        TransitionTo("Completed");

        if (CanSendNotification)
        {
            await context.PublishAsync(
                new CompleteOrderCommand(Id, FullName!, Email!, EffectiveTotalMoney)
            );
        }

        MarkCompleted();
    }

    /// <summary>
    /// Handles order cancellation. Transitions to Canceled and finalizes the saga.
    /// </summary>
    public async Task Handle(
        OrderStatusChangedToCancelIntegrationEvent @event,
        IMessageContext context
    )
    {
        if (CurrentState != "Placed")
        {
            return;
        }

        BasketId = @event.BasketId;
        Email = @event.Email;
        FullName = @event.FullName;
        TotalMoney = @event.TotalMoney;
        IsTimeoutCancelled = true;

        TransitionTo("Cancelled");

        if (CanSendNotification)
        {
            await context.PublishAsync(
                new CancelOrderCommand(Id, FullName!, Email!, EffectiveTotalMoney)
            );
        }

        MarkCompleted();
    }

    /// <summary>
    /// Handles placement timeout. Retries the order placement or transitions to Failed if max retries exceeded.
    /// </summary>
    public async Task Handle(
        PlaceOrderTimeoutIntegrationEvent @event,
        IMessageContext context,
        IOptions<OrderStateMachineSettings> options
    )
    {
        if (CurrentState != "Placed" || IsTimeoutCancelled)
        {
            return;
        }

        var settings = options.Value;
        TimeoutRetryCount++;

        if (TimeoutRetryCount < settings.MaxAttempts)
        {
            await context.ScheduleAsync(
                new PlaceOrderTimeoutIntegrationEvent(Id),
                settings.MaxRetryTimeout
            );

            await context.PublishAsync(
                new PlaceOrderCommand(BasketId, FullName, Email, Id, EffectiveTotalMoney)
            );
        }
        else
        {
            TransitionTo("Failed");

            if (CanSendNotification)
            {
                await context.PublishAsync(
                    new CancelOrderCommand(Id, FullName!, Email!, EffectiveTotalMoney)
                );
            }

            MarkCompleted();
        }
    }

    private void TransitionTo(string newState)
    {
        var previousState = CurrentState;

        _stateTransitionCounter.Add(1, new("from_state", previousState), new("to_state", newState));

        using var activity = _activitySource.StartActivity();
        activity?.SetTag("saga.order_id", Id);
        activity?.SetTag("saga.from_state", previousState);
        activity?.SetTag("saga.to_state", newState);

        CurrentState = newState;
    }
}

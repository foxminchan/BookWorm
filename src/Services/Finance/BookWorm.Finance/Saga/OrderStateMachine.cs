using BookWorm.Contracts;
using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Finance.Saga;

public sealed class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(ILoggerFactory loggerFactory, OrderStateMachineSettings settings)
    {
        var logger = loggerFactory.CreateLogger(nameof(OrderStateMachine));

        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCancelled, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => BasketDeletedFailed, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => BasketDeleted, x => x.CorrelateById(context => context.Message.OrderId));

        Schedule(
            () => PlaceOrderTimeoutSchedule,
            x => x.PlaceOrderTimeoutTokenId,
            s =>
            {
                s.Delay = settings.MaxRetryTimeout;
                s.Received = r => r.CorrelateById(context => context.Message.OrderId);
            }
        );

        Initially(
            When(OrderPlaced)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.OrderPlacedDate = DateTimeHelper.UtcNow();
                })
                .Then(context =>
                    logger.LogInformation(
                        "[{Event}] Order state machine started for {OrderId}",
                        nameof(OrderPlaced),
                        context.Message.OrderId
                    )
                )
                .TransitionTo(Placed)
                .Schedule(PlaceOrderTimeoutSchedule, context => new(context.Saga.OrderId))
                .Publish(context => new PlaceOrderCommand(
                    context.Saga.BasketId,
                    context.Saga.FullName,
                    context.Saga.Email,
                    context.Saga.OrderId,
                    context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                ))
        );

        During(
            Placed,
            Ignore(OrderPlaced),
            When(PlaceOrderTimeoutSchedule.Received)
                .Then(context => context.Saga.TimeoutRetryCount++)
                .If(
                    context => context.Saga.TimeoutRetryCount < settings.MaxAttempts,
                    retry =>
                        retry
                            .Then(context =>
                                logger.LogInformation(
                                    "[{Event}] Retrying order processing for {OrderId} - Attempt {RetryCount}",
                                    nameof(PlaceOrderTimeoutSchedule.Received),
                                    context.Message.OrderId,
                                    context.Saga.TimeoutRetryCount
                                )
                            )
                            .Schedule(
                                PlaceOrderTimeoutSchedule,
                                context => new(context.Saga.OrderId)
                            )
                            .Publish(context => new PlaceOrderCommand(
                                context.Saga.BasketId,
                                context.Saga.FullName,
                                context.Saga.Email,
                                context.Saga.OrderId,
                                context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                            ))
                )
                .If(
                    context => context.Saga.TimeoutRetryCount >= settings.MaxAttempts,
                    finalFailure =>
                        finalFailure
                            .Then(context =>
                                logger.LogError(
                                    "[{Event}] Order processing failed after {MaxRetryAttempts} timeout retries for {OrderId}",
                                    nameof(PlaceOrderTimeoutSchedule.Received),
                                    settings.MaxAttempts,
                                    context.Message.OrderId
                                )
                            )
                            .TransitionTo(Failed)
                            .If(
                                context =>
                                    !string.IsNullOrWhiteSpace(context.Saga.Email)
                                    && !string.IsNullOrWhiteSpace(context.Saga.FullName),
                                x =>
                                    x.Publish(context => new CancelOrderCommand(
                                        context.Saga.OrderId,
                                        context.Saga.FullName!,
                                        context.Saga.Email!,
                                        context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                                    ))
                            )
                            .Finalize()
                ),
            When(BasketDeletedFailed)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                })
                .Then(context =>
                    logger.LogError(
                        "[{Event}] Basket deletion failed for {OrderId}",
                        nameof(BasketDeletedFailed),
                        context.Message.OrderId
                    )
                )
                .TransitionTo(Failed)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .Publish(context => new DeleteBasketFailedCommand(
                    context.Saga.BasketId,
                    context.Saga.Email,
                    context.Saga.OrderId,
                    context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                )),
            When(BasketDeleted)
                .Then(context =>
                {
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                })
                .Then(context =>
                    logger.LogInformation(
                        "[{Event}] Basket deletion completed for {OrderId}",
                        nameof(BasketDeleted),
                        context.Message.OrderId
                    )
                )
                .Unschedule(PlaceOrderTimeoutSchedule)
                .Publish(context => new DeleteBasketCompleteCommand(
                    context.Saga.OrderId,
                    context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                )),
            When(OrderCompleted)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                })
                .Then(context =>
                    logger.LogInformation(
                        "[{Event}] Order state machine completed for {OrderId}",
                        nameof(OrderCompleted),
                        context.Message.OrderId
                    )
                )
                .TransitionTo(Completed)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .If(
                    context =>
                        !string.IsNullOrWhiteSpace(context.Saga.Email)
                        && !string.IsNullOrWhiteSpace(context.Saga.FullName),
                    x =>
                        x.Publish(context => new CompleteOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName!,
                            context.Saga.Email!,
                            context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                        ))
                )
                .Finalize(),
            When(OrderCancelled)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                })
                .Then(context =>
                    logger.LogInformation(
                        "[{Event}] Order state machine cancelled for {OrderId}",
                        nameof(OrderCancelled),
                        context.Message.OrderId
                    )
                )
                .TransitionTo(Cancelled)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .If(
                    context =>
                        !string.IsNullOrWhiteSpace(context.Saga.Email)
                        && !string.IsNullOrWhiteSpace(context.Saga.FullName),
                    x =>
                        x.Publish(context => new CancelOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName!,
                            context.Saga.Email!,
                            context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                        ))
                )
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public State Placed { get; set; } = null!;
    public State Completed { get; set; } = null!;
    public State Cancelled { get; set; } = null!;
    public State Failed { get; set; } = null!;

    public Event<UserCheckedOutIntegrationEvent> OrderPlaced { get; init; } = null!;

    public Event<OrderStatusChangedToCompleteIntegrationEvent> OrderCompleted { get; init; } =
        null!;

    public Event<OrderStatusChangedToCancelIntegrationEvent> OrderCancelled { get; init; } = null!;
    public Event<BasketDeletedFailedIntegrationEvent> BasketDeletedFailed { get; init; } = null!;
    public Event<BasketDeletedCompleteIntegrationEvent> BasketDeleted { get; init; } = null!;

    public Schedule<
        OrderState,
        PlaceOrderTimeoutIntegrationEvent
    > PlaceOrderTimeoutSchedule { get; init; } = null!;
}

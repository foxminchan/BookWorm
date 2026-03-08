using BookWorm.Contracts;
using BookWorm.Finance.Saga.Activities;

namespace BookWorm.Finance.Saga;

internal sealed class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(OrderStateMachineSettings settings)
    {
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
                .Activity(x => x.OfType<PlaceOrderActivity>())
                .TransitionTo(Placed)
                .Schedule(PlaceOrderTimeoutSchedule, context => new(context.Saga.OrderId))
                .Publish(context => new PlaceOrderCommand(
                    context.Saga.BasketId,
                    context.Saga.FullName,
                    context.Saga.Email,
                    context.Saga.OrderId,
                    context.Saga.EffectiveTotalMoney
                ))
        );

        During(
            Placed,
            Ignore(OrderPlaced),
            When(PlaceOrderTimeoutSchedule.Received)
                .Then(context => context.Saga.IncrementRetry())
                .If(
                    context => !context.Saga.HasExceededMaxRetries(settings.MaxAttempts),
                    retry =>
                        retry
                            .Schedule(
                                PlaceOrderTimeoutSchedule,
                                context => new(context.Saga.OrderId)
                            )
                            .Publish(context => new PlaceOrderCommand(
                                context.Saga.BasketId,
                                context.Saga.FullName,
                                context.Saga.Email,
                                context.Saga.OrderId,
                                context.Saga.EffectiveTotalMoney
                            ))
                )
                .If(
                    context => context.Saga.HasExceededMaxRetries(settings.MaxAttempts),
                    finalFailure =>
                        finalFailure
                            .TransitionTo(Failed)
                            .If(
                                context => context.Saga.CanSendNotification,
                                x =>
                                    x.Publish(context => new CancelOrderCommand(
                                        context.Saga.OrderId,
                                        context.Saga.FullName!,
                                        context.Saga.Email!,
                                        context.Saga.EffectiveTotalMoney
                                    ))
                            )
                            .Finalize()
                ),
            When(BasketDeletedFailed)
                .Activity(x => x.OfType<HandleBasketDeleteFailedActivity>())
                .TransitionTo(Failed)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .Publish(context => new DeleteBasketFailedCommand(
                    context.Saga.BasketId,
                    context.Saga.Email,
                    context.Saga.OrderId,
                    context.Saga.EffectiveTotalMoney
                )),
            When(BasketDeleted)
                .Activity(x => x.OfType<HandleBasketDeletedActivity>())
                .Unschedule(PlaceOrderTimeoutSchedule)
                .Publish(context => new DeleteBasketCompleteCommand(
                    context.Saga.OrderId,
                    context.Saga.EffectiveTotalMoney
                )),
            When(OrderCompleted)
                .Activity(x => x.OfType<CompleteOrderActivity>())
                .TransitionTo(Completed)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .If(
                    context => context.Saga.CanSendNotification,
                    x =>
                        x.Publish(context => new CompleteOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName!,
                            context.Saga.Email!,
                            context.Saga.EffectiveTotalMoney
                        ))
                )
                .Finalize(),
            When(OrderCancelled)
                .Activity(x => x.OfType<CancelOrderActivity>())
                .TransitionTo(Cancelled)
                .Unschedule(PlaceOrderTimeoutSchedule)
                .If(
                    context => context.Saga.CanSendNotification,
                    x =>
                        x.Publish(context => new CancelOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName!,
                            context.Saga.Email!,
                            context.Saga.EffectiveTotalMoney
                        ))
                )
                .Finalize()
        );

        During(
            Failed,
            Ignore(OrderPlaced),
            Ignore(OrderCompleted),
            Ignore(OrderCancelled),
            Ignore(BasketDeleted),
            Ignore(BasketDeletedFailed)
        );

        During(
            Completed,
            Ignore(OrderPlaced),
            Ignore(OrderCancelled),
            Ignore(BasketDeleted),
            Ignore(BasketDeletedFailed)
        );

        During(
            Cancelled,
            Ignore(OrderPlaced),
            Ignore(OrderCompleted),
            Ignore(BasketDeleted),
            Ignore(BasketDeletedFailed)
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

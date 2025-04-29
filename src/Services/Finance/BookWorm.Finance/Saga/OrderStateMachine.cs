using BookWorm.Contracts;

namespace BookWorm.Finance.Saga;

public sealed class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine(ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(OrderStateMachine));

        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCompleted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => OrderCancelled, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => BasketDeletedFailed, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => BasketDeleted, x => x.CorrelateById(context => context.Message.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderPlaced)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.TotalMoney = context.Message.TotalMoney;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.OrderPlacedDate = DateTime.UtcNow;

                    logger.LogInformation(
                        "[{Event}] Order placed for {OrderId}",
                        nameof(OrderPlaced),
                        context.Message.OrderId
                    );
                })
                .TransitionTo(Placed)
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
            When(BasketDeletedFailed)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.TotalMoney = context.Message.TotalMoney;

                    logger.LogInformation(
                        "[{Event}] Basket deletion failed for {OrderId}",
                        nameof(BasketDeletedFailed),
                        context.Message.OrderId
                    );
                })
                .TransitionTo(Failed)
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

                    logger.LogInformation(
                        "[{Event}] Basket deleted for {OrderId}",
                        nameof(BasketDeleted),
                        context.Message.OrderId
                    );
                })
                .Publish(context => new DeleteBasketCompleteCommand(
                    context.Saga.OrderId,
                    context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                ))
        );

        During(
            Placed,
            Ignore(OrderPlaced),
            When(OrderCompleted)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.TotalMoney = context.Message.TotalMoney;

                    logger.LogInformation(
                        "[{Event}] Order completed for {OrderId}",
                        nameof(OrderCompleted),
                        context.Message.OrderId
                    );
                })
                .TransitionTo(Completed)
                .If(
                    context => context.Saga.Email is not null,
                    x =>
                        x.Publish(context => new CompleteOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName,
                            context.Saga.Email,
                            context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                        ))
                ),
            When(OrderCancelled)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BasketId = context.Message.BasketId;
                    context.Saga.Email = context.Message.Email;
                    context.Saga.FullName = context.Message.FullName;
                    context.Saga.TotalMoney = context.Message.TotalMoney;

                    logger.LogInformation(
                        "[{Event}] Order cancelled for {OrderId}",
                        nameof(OrderCancelled),
                        context.Message.OrderId
                    );
                })
                .TransitionTo(Cancelled)
                .If(
                    context => context.Saga.Email is not null,
                    x =>
                        x.Publish(context => new CancelOrderCommand(
                            context.Saga.OrderId,
                            context.Saga.FullName,
                            context.Saga.Email,
                            context.Saga.TotalMoney.GetValueOrDefault(0.0M)
                        ))
                )
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
}

using BookWorm.Contracts;

namespace BookWorm.Finance.Saga.Activities;

public sealed class CancelOrderActivity(ILoggerFactory loggerFactory)
    : IStateMachineActivity<OrderState, OrderStatusChangedToCancelIntegrationEvent>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(CancelOrderActivity));

    public void Probe(ProbeContext context)
    {
        context.CreateScope("cancel-order");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<OrderState, OrderStatusChangedToCancelIntegrationEvent> context,
        IBehavior<OrderState, OrderStatusChangedToCancelIntegrationEvent> next
    )
    {
        context.Saga.MapFrom(context.Message);

        _logger.LogInformation(
            "[{Activity}] Order state machine cancelled for {OrderId}",
            nameof(CancelOrderActivity),
            context.Message.OrderId
        );

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<
            OrderState,
            OrderStatusChangedToCancelIntegrationEvent,
            TException
        > context,
        IBehavior<OrderState, OrderStatusChangedToCancelIntegrationEvent> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }
}

using BookWorm.Contracts;

namespace BookWorm.Finance.Saga.Activities;

public sealed class CompleteOrderActivity(ILoggerFactory loggerFactory)
    : IStateMachineActivity<OrderState, OrderStatusChangedToCompleteIntegrationEvent>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(CompleteOrderActivity));

    public void Probe(ProbeContext context)
    {
        context.CreateScope("complete-order");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<OrderState, OrderStatusChangedToCompleteIntegrationEvent> context,
        IBehavior<OrderState, OrderStatusChangedToCompleteIntegrationEvent> next
    )
    {
        context.Saga.MapFrom(context.Message);

        _logger.LogInformation(
            "[{Activity}] Order state machine completed for {OrderId}",
            nameof(CompleteOrderActivity),
            context.Message.OrderId
        );

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<
            OrderState,
            OrderStatusChangedToCompleteIntegrationEvent,
            TException
        > context,
        IBehavior<OrderState, OrderStatusChangedToCompleteIntegrationEvent> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }
}

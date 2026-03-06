using BookWorm.Contracts;

namespace BookWorm.Finance.Saga.Activities;

public sealed class HandleBasketDeletedActivity(ILoggerFactory loggerFactory)
    : IStateMachineActivity<OrderState, BasketDeletedCompleteIntegrationEvent>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(
        nameof(HandleBasketDeletedActivity)
    );

    public void Probe(ProbeContext context)
    {
        context.CreateScope("handle-basket-deleted");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<OrderState, BasketDeletedCompleteIntegrationEvent> context,
        IBehavior<OrderState, BasketDeletedCompleteIntegrationEvent> next
    )
    {
        context.Saga.MapFrom(context.Message);

        _logger.LogInformation(
            "[{Activity}] Basket deletion completed for {OrderId}",
            nameof(HandleBasketDeletedActivity),
            context.Message.OrderId
        );

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<
            OrderState,
            BasketDeletedCompleteIntegrationEvent,
            TException
        > context,
        IBehavior<OrderState, BasketDeletedCompleteIntegrationEvent> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }
}

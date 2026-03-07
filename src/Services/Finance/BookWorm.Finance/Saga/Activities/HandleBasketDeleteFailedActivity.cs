using BookWorm.Contracts;

namespace BookWorm.Finance.Saga.Activities;

public sealed class HandleBasketDeleteFailedActivity(ILoggerFactory loggerFactory)
    : IStateMachineActivity<OrderState, BasketDeletedFailedIntegrationEvent>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(
        nameof(HandleBasketDeleteFailedActivity)
    );

    public void Probe(ProbeContext context)
    {
        context.CreateScope("handle-basket-delete-failed");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<OrderState, BasketDeletedFailedIntegrationEvent> context,
        IBehavior<OrderState, BasketDeletedFailedIntegrationEvent> next
    )
    {
        context.Saga.MapFrom(context.Message);

        _logger.LogError(
            "[{Activity}] Basket deletion failed for {OrderId}",
            nameof(HandleBasketDeleteFailedActivity),
            context.Message.OrderId
        );

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<
            OrderState,
            BasketDeletedFailedIntegrationEvent,
            TException
        > context,
        IBehavior<OrderState, BasketDeletedFailedIntegrationEvent> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }
}

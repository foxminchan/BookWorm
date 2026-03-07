using BookWorm.Contracts;
using BookWorm.SharedKernel.Helpers;

namespace BookWorm.Finance.Saga.Activities;

public sealed class PlaceOrderActivity(ILoggerFactory loggerFactory)
    : IStateMachineActivity<OrderState, UserCheckedOutIntegrationEvent>
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(PlaceOrderActivity));

    public void Probe(ProbeContext context)
    {
        context.CreateScope("place-order");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<OrderState, UserCheckedOutIntegrationEvent> context,
        IBehavior<OrderState, UserCheckedOutIntegrationEvent> next
    )
    {
        context.Saga.MapFrom(context.Message);
        context.Saga.OrderPlacedDate = DateTimeHelper.UtcNow();

        _logger.LogInformation(
            "[{Activity}] Order state machine started for {OrderId}",
            nameof(PlaceOrderActivity),
            context.Message.OrderId
        );

        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<OrderState, UserCheckedOutIntegrationEvent, TException> context,
        IBehavior<OrderState, UserCheckedOutIntegrationEvent> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }
}

using BookWorm.Contracts;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

public sealed class DeleteBasketFailedCommandHandler(IOrderRepository repository)
    : IConsumer<DeleteBasketFailedCommand>
{
    public async Task Consume(ConsumeContext<DeleteBasketFailedCommand> context)
    {
        var message = context.Message;

        var order = await repository.GetByIdAsync(message.OrderId, context.CancellationToken);

        if (order is null)
        {
            return;
        }

        repository.Delete(order);

        await repository.UnitOfWork.SaveEntitiesAsync(context.CancellationToken);
    }
}

[ExcludeFromCodeCoverage]
public sealed class DeleteBasketFailedCommandHandlerDefinition
    : ConsumerDefinition<DeleteBasketFailedCommandHandler>
{
    public DeleteBasketFailedCommandHandlerDefinition()
    {
        Endpoint(x => x.Name = "basket-checkout-failed");
        ConcurrentMessageLimit = 1;
    }
}

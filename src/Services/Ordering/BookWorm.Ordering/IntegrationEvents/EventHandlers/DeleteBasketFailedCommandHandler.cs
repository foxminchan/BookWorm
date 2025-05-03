using BookWorm.Contracts;
using Saunter.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

[AsyncApi]
public sealed class DeleteBasketFailedCommandHandler(IOrderRepository repository)
    : IConsumer<DeleteBasketFailedCommand>
{
    [Channel("basket-checkout-failed")]
    [PublishOperation(
        typeof(DeleteBasketFailedCommand),
        OperationId = nameof(DeleteBasketFailedCommand),
        Summary = "Delete basket failed",
        Description = "Represents a failed integration event when deleting a basket in the system"
    )]
    public async Task Consume(ConsumeContext<DeleteBasketFailedCommand> context)
    {
        var message = context.Message;

        var order = await repository.GetByIdAsync(message.OrderId, CancellationToken.None);

        if (order is null)
        {
            return;
        }

        repository.Delete(order);

        await repository.UnitOfWork.SaveEntitiesAsync(CancellationToken.None);
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

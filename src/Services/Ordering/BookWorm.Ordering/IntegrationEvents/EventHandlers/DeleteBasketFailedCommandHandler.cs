using BookWorm.Contracts;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

public sealed class DeleteBasketFailedCommandHandler(IOrderRepository repository)
{
    public async Task Handle(DeleteBasketFailedCommand message, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(message.OrderId, cancellationToken);

        if (order is null)
        {
            return;
        }

        order.Delete();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}

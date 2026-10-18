using BookWorm.Contracts;
using Wolverine.Attributes;

namespace BookWorm.Ordering.IntegrationEvents.EventHandlers;

internal sealed class DeleteBasketFailedCommandHandler(IOrderRepository repository)
{
    [WolverineHandler]
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

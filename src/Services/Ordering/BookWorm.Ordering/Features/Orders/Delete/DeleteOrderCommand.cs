using BookWorm.Chassis.CQRS;
using BookWorm.Chassis.Utilities.Guards;
using Mediator;

namespace BookWorm.Ordering.Features.Orders.Delete;

[Transactional]
public sealed record DeleteOrderCommand(Guid Id) : ICommand;

internal sealed class DeleteOrderHandler(IOrderRepository repository)
    : ICommandHandler<DeleteOrderCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);

        Guard.Against.NotFound(order, request.Id);

        order.Delete();

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}

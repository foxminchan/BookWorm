namespace BookWorm.Ordering.Features.Orders.Delete;

public sealed record DeleteOrderCommand(Guid Id) : ICommand;

public sealed class DeleteOrderHandler(IOrderRepository repository)
    : ICommandHandler<DeleteOrderCommand>
{
    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order with id {request.Id} not found.");
        }

        order.Delete();
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}

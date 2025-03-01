namespace BookWorm.Ordering.Features.Buyers.Delete;

public sealed record DeleteBuyerCommand(Guid Id) : ICommand;

public sealed class DeleteBuyerHandler(IBuyerRepository repository)
    : ICommandHandler<DeleteBuyerCommand>
{
    public async Task<Unit> Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
    {
        var buyer = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (buyer is null)
        {
            throw new NotFoundException($"Buyer with ID {request.Id} not found.");
        }

        repository.Delete(buyer);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Unit.Value;
    }
}
